from fastapi import APIRouter, Depends
from sqlalchemy.orm import Session
from app.database import get_db
from app import models, schemas, auth
from app.deps import get_current_user

router = APIRouter(prefix="/api/users", tags=["users"])


@router.get("/me")
def get_me(user: models.User = Depends(get_current_user)):
    return schemas.ok(schemas.UserOut.model_validate(user).model_dump())


@router.put("/me")
def update_me(
    payload: dict,
    user: models.User = Depends(get_current_user),
    db: Session = Depends(get_db),
):
    if "fullName" in payload and payload["fullName"]:
        user.full_name = payload["fullName"]
    if "skills" in payload:
        user.skills = payload["skills"]
    if "phone" in payload:
        user.phone = payload["phone"]
    if "avatarUrl" in payload:
        user.avatar_url = payload["avatarUrl"]

    db.commit()
    db.refresh(user)
    return schemas.ok(schemas.UserOut.model_validate(user).model_dump(),
                      "Профиль обновлён")


@router.put("/me/password")
def change_password(
    payload: dict,
    user: models.User = Depends(get_current_user),
    db: Session = Depends(get_db),
):
    old_password = payload.get("oldPassword")
    new_password = payload.get("newPassword")

    if not old_password or not new_password:
        return schemas.fail("Заполните все поля")
    if len(new_password) < 8:
        return schemas.fail("Новый пароль минимум 8 символов")
    if not auth.verify_password(old_password, user.password_hash):
        return schemas.fail("Старый пароль неверный")

    user.password_hash = auth.hash_password(new_password)
    db.commit()
    return schemas.ok(None, "Пароль успешно изменён")


@router.get("/{user_id}/public")
def get_public_profile(
    user_id: int,
    db: Session = Depends(get_db),
):
    target = db.query(models.User).get(user_id)
    if not target or not target.is_active:
        return schemas.fail("Пользователь не найден")

    # Команды, в которых состоит пользователь
    memberships = (
        db.query(models.TeamMember)
        .filter(models.TeamMember.user_id == user_id)
        .all()
    )

    participations = []
    for m in memberships:
        team = m.team
        if not team:
            continue
        h = team.hackathon
        participations.append({
            "hackathonId": h.id if h else None,
            "hackathonTitle": h.title if h else None,
            "teamId": team.id,
            "teamName": team.name,
            "role": "капитан" if team.captain_id == user_id else "участник",
            "status": h.status if h else None,
        })

    return schemas.ok({
        "id": target.id,
        "fullName": target.full_name,
        "email": target.email,
        "phone": target.phone,
        "skills": target.skills,
        "avatarUrl": target.avatar_url,
        "role": target.role,
        "createdAt": target.created_at,
        "participations": participations,
    })