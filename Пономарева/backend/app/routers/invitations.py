from fastapi import APIRouter, Depends
from sqlalchemy.orm import Session
from app.database import get_db
from app import models, schemas
from app.deps import get_current_user, require_role

router = APIRouter(prefix="/api/invitations", tags=["invitations"])


@router.post("/team/{team_id}/user/{user_id}")
def send_invitation(
    team_id: int,
    user_id: int,
    user: models.User = Depends(get_current_user),
    db: Session = Depends(get_db),
):
    team = db.query(models.Team).get(team_id)
    if not team:
        return schemas.fail("Команда не найдена")
    if team.captain_id != user.id:
        return schemas.fail("Только капитан может приглашать")

    target = db.query(models.User).get(user_id)
    if not target:
        return schemas.fail("Пользователь не найден")
    if target.role != "participant":
        return schemas.fail("Приглашать можно только участников")

    existing_member = db.query(models.TeamMember).filter_by(
        team_id=team_id, user_id=user_id
    ).first()
    if existing_member:
        return schemas.fail("Пользователь уже в команде")

    existing = db.query(models.TeamInvitation).filter_by(
        team_id=team_id, user_id=user_id
    ).first()
    if existing:
        if existing.status == "pending":
            return schemas.fail("Приглашение уже отправлено")
        existing.status = "pending"
        db.commit()
        return schemas.ok(None, "Приглашение отправлено повторно")

    inv = models.TeamInvitation(team_id=team_id, user_id=user_id, status="pending")
    db.add(inv)
    db.commit()
    return schemas.ok(None, "Приглашение отправлено")


@router.get("/my")
def my_invitations(
    user: models.User = Depends(get_current_user),
    db: Session = Depends(get_db),
):
    items = (
        db.query(models.TeamInvitation)
        .filter(models.TeamInvitation.user_id == user.id,
                models.TeamInvitation.status == "pending")
        .all()
    )

    result = []
    for i in items:
        result.append({
            "id": i.id,
            "teamId": i.team_id,
            "teamName": i.team.name if i.team else None,
            "hackathonTitle": i.team.hackathon.title if i.team and i.team.hackathon else None,
            "fromUserName": i.team.members[0].user.full_name if i.team and i.team.members else None,
            "status": i.status,
            "createdAt": i.created_at,
        })
    return result


@router.post("/{inv_id}/accept")
def accept_invitation(
    inv_id: int,
    user: models.User = Depends(get_current_user),
    db: Session = Depends(get_db),
):
    inv = db.query(models.TeamInvitation).get(inv_id)
    if not inv or inv.user_id != user.id:
        return schemas.fail("Приглашение не найдено")
    if inv.status != "pending":
        return schemas.fail("Приглашение уже обработано")

    team = db.query(models.Team).get(inv.team_id)
    if not team:
        return schemas.fail("Команда не найдена")
    if len(team.members) >= 5:
        return schemas.fail("Команда заполнена (макс. 5 человек)")

    db.add(models.TeamMember(team_id=inv.team_id, user_id=user.id))
    inv.status = "accepted"
    db.commit()
    return schemas.ok(None, "Вы вступили в команду")


@router.post("/{inv_id}/reject")
def reject_invitation(
    inv_id: int,
    user: models.User = Depends(get_current_user),
    db: Session = Depends(get_db),
):
    inv = db.query(models.TeamInvitation).get(inv_id)
    if not inv or inv.user_id != user.id:
        return schemas.fail("Приглашение не найдено")

    inv.status = "rejected"
    db.commit()
    return schemas.ok(None, "Приглашение отклонено")


@router.post("/request/team/{team_id}")
def request_join(
    team_id: int,
    user: models.User = Depends(get_current_user),
    db: Session = Depends(get_db),
):
    team = db.query(models.Team).get(team_id)
    if not team:
        return schemas.fail("Команда не найдена")

    existing_member = db.query(models.TeamMember).filter_by(
        team_id=team_id, user_id=user.id
    ).first()
    if existing_member:
        return schemas.fail("Вы уже в команде")

    existing = db.query(models.TeamInvitation).filter_by(
        team_id=team_id, user_id=user.id
    ).first()
    if existing:
        if existing.status == "pending":
            return schemas.fail("Заявка уже отправлена")
        existing.status = "pending"
        db.commit()
        return schemas.ok(None, "Заявка отправлена повторно")

    inv = models.TeamInvitation(team_id=team_id, user_id=user.id, status="pending")
    db.add(inv)
    db.commit()
    return schemas.ok(None, "Заявка отправлена капитану")


@router.get("/team/{team_id}/pending")
def team_pending_requests(
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