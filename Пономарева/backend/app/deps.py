from fastapi import Depends, HTTPException
from fastapi.security import HTTPBearer, HTTPAuthorizationCredentials
from sqlalchemy.orm import Session
from app.database import get_db
from app.auth import decode_token
from app import models

security = HTTPBearer(auto_error=False)


def get_current_user(
    credentials: HTTPAuthorizationCredentials = Depends(security),
    db: Session = Depends(get_db),
) -> models.User:
    if not credentials:
        raise HTTPException(status_code=401, detail="Требуется авторизация")
    payload = decode_token(credentials.credentials)
    if not payload:
        raise HTTPException(status_code=401, detail="Недействительный токен")
    user = db.query(models.User).filter(models.User.id == int(payload["sub"])).first()
    if not user or not user.is_active:
        raise HTTPException(status_code=401, detail="Пользователь не найден")
    return user


def require_role(*roles: str):
    def checker(user: models.User = Depends(get_current_user)) -> models.User:
        if user.role not in roles:
            raise HTTPException(status_code=403, detail="Недостаточно прав")
        return user
    return checker