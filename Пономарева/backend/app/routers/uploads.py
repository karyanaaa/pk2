from fastapi import APIRouter, Depends, UploadFile, File, Request
from fastapi.responses import FileResponse
import os
import uuid
from app.deps import get_current_user
from app import models, schemas

router = APIRouter(prefix="/api/uploads", tags=["uploads"])

AVATAR_DIR = os.path.join("uploads", "avatars")
os.makedirs(AVATAR_DIR, exist_ok=True)


@router.post("/avatar")
def upload_avatar(
    request: Request,
    file: UploadFile = File(...),
    user: models.User = Depends(get_current_user),
):
    raw_name = file.filename or "avatar"
    raw_name = raw_name.replace("\\", "/").split("/")[-1]
    ext = os.path.splitext(raw_name)[1].lower()
    print(f"[UPLOAD] filename='{file.filename}', ext='{ext}'")

    if ext not in (".png", ".jpg", ".jpeg", ".gif", ".webp"):
        return schemas.fail(f"Недопустимый формат: '{ext}'. Разрешены .png, .jpg, .jpeg, .gif, .webp")

    filename = f"{user.id}_{uuid.uuid4().hex}{ext}"
    path = os.path.join(AVATAR_DIR, filename)

    with open(path, "wb") as f:
        f.write(file.file.read())

    # полный URL — клиент сможет открыть напрямую
    base = str(request.base_url).rstrip("/")   # http://localhost:8080/
    url = f"{base}/uploads/avatars/{filename}"

    return schemas.ok({"url": url}, "Аватар загружен")


@router.get("/avatar/{filename}")
def get_avatar(filename: str):
    path = os.path.join(AVATAR_DIR, filename)
    if not os.path.exists(path):
        return {"error": "not found"}
    return FileResponse(path)