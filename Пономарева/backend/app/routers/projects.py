from fastapi import APIRouter, Depends
from sqlalchemy.orm import Session
from datetime import datetime
from app.database import get_db
from app import models, schemas
from app.deps import get_current_user, require_role

router = APIRouter(prefix="/api/projects", tags=["projects"])


def _proj_out(p: models.Project) -> dict:
    hackathon_id = None
    hackathon_title = None
    if p.team and p.team.hackathon:
        hackathon_id = p.team.hackathon_id
        hackathon_title = p.team.hackathon.title

    return {
        "id": p.id,
        "teamId": p.team_id,
        "hackathonId": hackathon_id,
        "hackathonTitle": hackathon_title,
        "title": p.title,
        "description": p.description,
        "repositoryUrl": p.repository_url,
        "filePath": p.file_path,
        "presentationPath": p.presentation_path,
        "uploadedAt": p.uploaded_at,
        "teamName": p.team.name if p.team else None,
    }


@router.post("")
def upload_project(
    payload: schemas.ProjectIn,
    user: models.User = Depends(require_role("participant", "admin")),
    db: Session = Depends(get_db),
):
    team = db.query(models.Team).get(payload.teamId)
    if not team:
        return schemas.fail("Команда не найдена")
    if team.captain_id != user.id and user.role != "admin":
        return schemas.fail("Только капитан может загрузить проект")

    hackathon = team.hackathon

    # 🔴 Проверка: хакатон ещё не начался — загружать нельзя
    if datetime.now() < hackathon.start_date:
        return schemas.fail(
            f"Загрузка проекта откроется {hackathon.start_date:%d.%m.%Y %H:%M} — "
            "дождитесь старта хакатона"
        )

    deadline = hackathon.submission_deadline or hackathon.end_date
    if deadline and datetime.now() > deadline:
        return schemas.fail("Приём работ завершён")

    existing = db.query(models.Project).filter_by(team_id=payload.teamId).first()
    if existing:
        existing.title = payload.title
        existing.description = payload.description
        existing.repository_url = payload.repositoryUrl
        existing.file_path = payload.filePath
        existing.presentation_path = payload.presentationPath
        existing.uploaded_at = datetime.now()
        db.commit()
        db.refresh(existing)
        return schemas.ok(_proj_out(existing), "Проект обновлён")

    p = models.Project(
        team_id=payload.teamId,
        title=payload.title,
        description=payload.description,
        repository_url=payload.repositoryUrl,
        file_path=payload.filePath,
        presentation_path=payload.presentationPath,
    )
    db.add(p)
    db.commit()
    db.refresh(p)
    return schemas.ok(_proj_out(p), "Проект загружен")

    p = models.Project(
        team_id=payload.teamId,
        title=payload.title,
        description=payload.description,
        repository_url=payload.repositoryUrl,
        file_path=payload.filePath,
        presentation_path=payload.presentationPath,
    )
    db.add(p)
    db.commit()
    db.refresh(p)
    return schemas.ok(_proj_out(p), "Проект загружен")


@router.get("/for-jury")
def for_jury(
    user: models.User = Depends(require_role("jury", "organizer", "admin")),
    db: Session = Depends(get_db),
):
    projects = db.query(models.Project).order_by(models.Project.uploaded_at.desc()).all()
    return [_proj_out(p) for p in projects]


@router.get("/my")
def my_projects(
    user: models.User = Depends(get_current_user),
    db: Session = Depends(get_db),
):
    team_ids = [
        m.team_id
        for m in db.query(models.TeamMember).filter(models.TeamMember.user_id == user.id).all()
    ]

    if not team_ids:
        return []

    projects = (
        db.query(models.Project)
        .filter(models.Project.team_id.in_(team_ids))
        .order_by(models.Project.uploaded_at.desc())
        .all()
    )
    return [_proj_out(p) for p in projects]