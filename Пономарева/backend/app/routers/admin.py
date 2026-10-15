from fastapi import APIRouter, Depends
from sqlalchemy.orm import Session
from app.database import get_db
from app import models, schemas, auth as auth_module
from app.deps import require_role

router = APIRouter(prefix="/api/admin", tags=["admin"])


@router.get("/users")
def list_users(
    user: models.User = Depends(require_role("admin")),
    db: Session = Depends(get_db),
):
    users = db.query(models.User).order_by(models.User.id).all()
    return [schemas.UserOut.model_validate(u).model_dump() for u in users]


@router.post("/users")
def create_user(
    payload: dict,
    user: models.User = Depends(require_role("admin")),
    db: Session = Depends(get_db),
):
    full_name = payload.get("fullName")
    email = payload.get("email")
    password = payload.get("password")
    role = payload.get("role", "participant")

    if not full_name or not email or not password:
        return schemas.fail("Заполните все поля")
    if len(password) < 8:
        return schemas.fail("Пароль минимум 8 символов")
    if role not in ("admin", "organizer", "participant", "jury"):
        return schemas.fail("Недопустимая роль")

    if db.query(models.User).filter(models.User.email == email).first():
        return schemas.fail("Пользователь с таким email уже существует")

    new_user = models.User(
        full_name=full_name,
        email=email,
        password_hash=auth_module.hash_password(password),
        role=role,
        is_active=True,
    )
    db.add(new_user)
    db.commit()
    db.refresh(new_user)

    return schemas.ok(
        schemas.UserOut.model_validate(new_user).model_dump(),
        f"Пользователь создан с ролью '{role}'",
    )


@router.put("/users/{user_id}/role")
def change_role(
    user_id: int,
    payload: dict,
    user: models.User = Depends(require_role("admin")),
    db: Session = Depends(get_db),
):
    target = db.query(models.User).get(user_id)
    if not target:
        return schemas.fail("Пользователь не найден")

    new_role = payload.get("role")
    if new_role not in ("participant", "organizer", "jury", "admin"):
        return schemas.fail("Недопустимая роль")

    target.role = new_role
    db.commit()
    db.refresh(target)

    return schemas.ok(
        schemas.UserOut.model_validate(target).model_dump(),
        f"Роль изменена на {new_role}",
    )


@router.put("/users/{user_id}/block")
def block_user(
    user_id: int,
    user: models.User = Depends(require_role("admin")),
    db: Session = Depends(get_db),
):
    target = db.query(models.User).get(user_id)
    if not target:
        return schemas.fail("Пользователь не найден")
    target.is_active = False
    db.commit()
    return schemas.ok(None, "Пользователь заблокирован")


@router.put("/users/{user_id}/unblock")
def unblock_user(
    user_id: int,
    user: models.User = Depends(require_role("admin")),
    db: Session = Depends(get_db),
):
    target = db.query(models.User).get(user_id)
    if not target:
        return schemas.fail("Пользователь не найден")
    target.is_active = True
    db.commit()
    return schemas.ok(None, "Пользователь разблокирован")