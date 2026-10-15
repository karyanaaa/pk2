from fastapi import APIRouter, Depends
from sqlalchemy.orm import Session
from app.database import get_db
from app import models, schemas
from app.deps import get_current_user

router = APIRouter(prefix="/api/chat", tags=["chat"])


def _user_team_in_hackathon(db, hackathon_id, user_id):
    """Возвращает команду пользователя в этом хакатоне или None."""
    return (
        db.query(models.Team)
        .join(models.TeamMember, models.TeamMember.team_id == models.Team.id)
        .filter(
            models.Team.hackathon_id == hackathon_id,
            models.TeamMember.user_id == user_id,
        )
        .first()
    )


@router.get("/my-hackathons")
def my_chat_hackathons(
    user: models.User = Depends(get_current_user),
    db: Session = Depends(get_db),
):
    """Список хакатонов, где пользователь состоит в команде."""
    hackathons = (
        db.query(models.Hackathon)
        .join(models.Team, models.Team.hackathon_id == models.Hackathon.id)
        .join(models.TeamMember, models.TeamMember.team_id == models.Team.id)
        .filter(models.TeamMember.user_id == user.id)
        .distinct()
        .all()
    )

    result = []
    for h in hackathons:
        team_names = [
            t.name for t in h.teams if any(m.user_id == user.id for m in t.members)
        ]
        result.append({
            "id": h.id,
            "title": h.title,
            "teamNames": team_names,
        })
    return result


@router.get("/{hackathon_id}")
def get_messages(
    hackathon_id: int,
    user: models.User = Depends(get_current_user),
    db: Session = Depends(get_db),
):
    # доступ только участникам команды этого хакатона
    if not _user_team_in_hackathon(db, hackathon_id, user.id):
        return schemas.fail("Чат доступен только участникам команд этого хакатона")

    msgs = (
        db.query(models.ChatMessage)
        .filter(models.ChatMessage.hackathon_id == hackathon_id)
        .order_by(models.ChatMessage.created_at.asc())
        .limit(200)
        .all()
    )

    return [
        {
            "id": m.id,
            "userId": m.user_id,
            "userName": m.user.full_name if m.user else "?",
            "text": m.text,
            "createdAt": m.created_at,
        }
        for m in msgs
    ]


@router.post("/{hackathon_id}")
def send_message(
    hackathon_id: int,
    payload: schemas.ChatMessageIn,
    user: models.User = Depends(get_current_user),
    db: Session = Depends(get_db),
):
    if not _user_team_in_hackathon(db, hackathon_id, user.id):
        return schemas.fail("Чат доступен только участникам команд этого хакатона")

    text = (payload.text or "").strip()
    if not text:
        return schemas.fail("Сообщение пустое")
    if len(text) > 1000:
        return schemas.fail("Слишком длинное сообщение")

    msg = models.ChatMessage(
        hackathon_id=hackathon_id,
        user_id=user.id,
        text=text,
    )
    db.add(msg)
    db.commit()
    db.refresh(msg)

    return schemas.ok({
        "id": msg.id,
        "userId": msg.user_id,
        "userName": user.full_name,
        "text": msg.text,
        "createdAt": msg.created_at,
    }, "Сообщение отправлено")