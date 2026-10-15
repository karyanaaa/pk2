from fastapi import APIRouter, Depends
from sqlalchemy.orm import Session
from datetime import datetime
from app.database import get_db
from app import models, schemas
from app.deps import get_current_user, require_role

router = APIRouter(prefix="/api", tags=["teams"])


def _team_out(t: models.Team) -> dict:
    proj = t.project
    h = t.hackathon
    deadline = h.submission_deadline or h.end_date
    now = datetime.now()
    if proj is not None:
        sub = "submitted"; hours = None
    elif now > deadline:
        sub = "missed"; hours = 0
    else:
        sub = "pending"; hours = int((deadline - now).total_seconds() // 3600)

    return {
        "id": t.id,
        "hackathonId": t.hackathon_id,
        "captainId": t.captain_id,
        "name": t.name,
        "description": t.description,
        "createdAt": t.created_at,
        "hackathonTitle": t.hackathon.title if t.hackathon else None,
        "captainName": next(
            (m.user.full_name for m in t.members if m.user_id == t.captain_id), None
        ),
        "members": [
            schemas.UserOut.model_validate(m.user).model_dump() for m in t.members
        ],
        "submissionStatus": sub,
        "hoursLeft": hours,
    }


@router.get("/hackathons/{hid}/teams")
def list_teams(hid: int, db: Session = Depends(get_db)):
    teams = db.query(models.Team).filter(models.Team.hackathon_id == hid).all()
    return [_team_out(t) for t in teams]


@router.post("/teams")
def create_team(
    payload: schemas.TeamIn,
    user: models.User = Depends(require_role("participant", "admin")),
    db: Session = Depends(get_db),
):
    hackathon = db.query(models.Hackathon).get(payload.hackathonId)
    if not hackathon:
        return schemas.fail("Хакатон не найден")
    if hackathon.registration_deadline < datetime.now():
        return schemas.fail("Регистрация на хакатон закрыта")

    # проверка регистрации на хакатон
    if user.role != "admin":
        reg = db.query(models.HackathonRegistration).filter_by(
            hackathon_id=payload.hackathonId, user_id=user.id, status="approved"
        ).first()
        if not reg:
            return schemas.fail("Вы не зарегистрированы на этот хакатон (заявка не одобрена)")

    exists = db.query(models.Team).filter(
        models.Team.hackathon_id == payload.hackathonId,
        models.Team.name == payload.name,
    ).first()
    if exists:
        return schemas.fail("Такое название команды уже занято")

    team = models.Team(
        hackathon_id=payload.hackathonId,
        captain_id=user.id,
        name=payload.name,
        description=payload.description,
    )
    db.add(team)
    db.flush()
    db.add(models.TeamMember(team_id=team.id, user_id=user.id))
    db.commit()
    db.refresh(team)
    return schemas.ok(_team_out(team), "Команда создана")


@router.post("/teams/{team_id}/join")
def join_team(
    team_id: int,
    user: models.User = Depends(require_role("participant", "admin")),
    db: Session = Depends(get_db),
):
    team = db.query(models.Team).get(team_id)
    if not team:
        return schemas.fail("Команда не найдена")

    existing = db.query(models.TeamMember).filter_by(
        team_id=team_id, user_id=user.id
    ).first()
    if existing:
        return schemas.fail("Вы уже в этой команде")
    if len(team.members) >= 5:
        return schemas.fail("Превышен лимит команды (макс. 5 человек)")

    db.add(models.TeamMember(team_id=team_id, user_id=user.id))
    db.commit()
    return schemas.ok(None, "Вы вступили в команду")


@router.delete("/teams/{team_id}/leave")
def leave_team(
    team_id: int,
    user: models.User = Depends(get_current_user),
    db: Session = Depends(get_db),
):
    m = db.query(models.TeamMember).filter_by(
        team_id=team_id, user_id=user.id
    ).first()
    if not m:
        return schemas.fail("Вы не состоите в этой команде")
    team = db.query(models.Team).get(team_id)
    if team.captain_id == user.id:
        return schemas.fail("Капитан не может покинуть команду. Передайте капитанство.")
    db.delete(m)
    db.commit()
    return schemas.ok(None, "Вы покинули команду")


@router.get("/my/teams")
def my_teams(
    user: models.User = Depends(get_current_user),
    db: Session = Depends(get_db),
):
    team_ids = [
        m.team_id
        for m in db.query(models.TeamMember).filter(models.TeamMember.user_id == user.id).all()
    ]
    if not team_ids:
        return []
    teams = db.query(models.Team).filter(models.Team.id.in_(team_ids)).all()
    return [_team_out(t) for t in teams]


@router.get("/teams/{team_id}/captain")
def team_captain(team_id: int, db: Session = Depends(get_db)):
    team = db.query(models.Team).get(team_id)
    if not team:
        return schemas.fail("Команда не найдена")
    return {
        "captainId": team.captain_id,
        "captainName": next(
            (m.user.full_name for m in team.members if m.user_id == team.captain_id),
            None,
        ),
    }


@router.get("/teams/{team_id}/pending-requests")
def pending_requests(
    team_id: int,
    user: models.User = Depends(get_current_user),
    db: Session = Depends(get_db),
):
    team = db.query(models.Team).get(team_id)
    if not team or team.captain_id != user.id:
        return schemas.fail("Только капитан видит заявки")

    items = db.query(models.TeamInvitation).filter_by(
        team_id=team_id, status="pending"
    ).all()

    return [
        {
            "id": i.id,
            "userId": i.user_id,
            "userName": i.user.full_name if i.user else None,
            "userEmail": i.user.email if i.user else None,
            "createdAt": i.created_at,
        }
        for i in items
    ]


@router.post("/team-invitations/{inv_id}/accept")
def accept_request(
    inv_id: int,
    user: models.User = Depends(get_current_user),
    db: Session = Depends(get_db),
):
    inv = db.query(models.TeamInvitation).get(inv_id)
    if not inv:
        return schemas.fail("Заявка не найдена")

    team = db.query(models.Team).get(inv.team_id)
    if not team or team.captain_id != user.id:
        return schemas.fail("Только капитан может принимать")

    if inv.status != "pending":
        return schemas.fail("Заявка уже обработана")

    existing = db.query(models.TeamMember).filter_by(
        team_id=inv.team_id, user_id=inv.user_id
    ).first()
    if existing:
        inv.status = "accepted"
        db.commit()
        return schemas.ok(None, "Пользователь уже в команде")

    if len(team.members) >= 5:
        return schemas.fail("Команда заполнена")

    db.add(models.TeamMember(team_id=inv.team_id, user_id=inv.user_id))
    inv.status = "accepted"
    db.commit()
    return schemas.ok(None, "Заявка принята")


@router.post("/team-invitations/{inv_id}/reject")
def reject_request(
    inv_id: int,
    user: models.User = Depends(get_current_user),
    db: Session = Depends(get_db),
):
    inv = db.query(models.TeamInvitation).get(inv_id)
    if not inv:
        return schemas.fail("Заявка не найдена")

    team = db.query(models.Team).get(inv.team_id)
    if not team or team.captain_id != user.id:
        return schemas.fail("Только капитан может отклонять")

    inv.status = "rejected"
    db.commit()
    return schemas.ok(None, "Заявка отклонена")