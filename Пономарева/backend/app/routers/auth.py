from fastapi import APIRouter, Depends
from sqlalchemy.orm import Session
from datetime import datetime, timedelta, timezone
from app.database import get_db
from app import models, schemas, auth

router = APIRouter(prefix="/api/auth", tags=["auth"])


@router.post("/register")
def register(payload: schemas.RegisterRequest, db: Session = Depends(get_db)):
    if db.query(models.User).filter(models.User.email == payload.email).first():
        return schemas.fail("Пользователь с таким email уже существует")

    user = models.User(
        full_name=payload.fullName,
        email=payload.email,
        password_hash=auth.hash_password(payload.password),
        role="participant",
    )
    db.add(user)
    db.commit()
    db.refresh(user)

    token = auth.create_access_token(user.id, user.role)
    return schemas.ok(
        {
            "token": token,
            "user": schemas.UserOut.model_validate(user).model_dump(),
        },
        "Регистрация успешна",
    )


@router.post("/login")
def login(payload: schemas.LoginRequest, db: Session = Depends(get_db)):
    user = db.query(models.User).filter(models.User.email == payload.email).first()
    if not user or not auth.verify_password(payload.password, user.password_hash):
        return schemas.fail("Неверный email или пароль")
    if not user.is_active:
        return schemas.fail("Аккаунт деактивирован")

    token = auth.create_access_token(user.id, user.role)
    return schemas.ok(
        {
            "token": token,
            "user": schemas.UserOut.model_validate(user).model_dump(),
        },
        "Вход выполнен",
    )


@router.post("/forgot-password")
def forgot_password(payload: schemas.ForgotPasswordRequest, db: Session = Depends(get_db)):
    user = db.query(models.User).filter(models.User.email == payload.email).first()
    if not user:
        return schemas.fail("Пользователь с таким email не найден")

    token = auth.generate_reset_token()
    user.reset_token = token
    user.reset_token_expiry = datetime.now(timezone.utc) + timedelta(hours=1)
    db.commit()

    return schemas.ok({"resetToken": token}, "Токен сброса сгенерирован")


@router.post("/reset-password")
def reset_password(payload: schemas.ResetPasswordRequest, db: Session = Depends(get_db)):
    user = db.query(models.User).filter(models.User.reset_token == payload.token).first()
    if not user:
        return schemas.fail("Неверный токен сброса")

    expiry = user.reset_token_expiry
    if expiry and expiry.tzinfo is None:
        expiry = expiry.replace(tzinfo=timezone.utc)
    if not expiry or expiry < datetime.now(timezone.utc):
        return schemas.fail("Срок действия токена истёк")

    user.password_hash = auth.hash_password(payload.newPassword)
    user.reset_token = None
    user.reset_token_expiry = None
    db.commit()
    return schemas.ok(None, "Пароль успешно изменён")