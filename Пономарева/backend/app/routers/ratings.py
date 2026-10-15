from fastapi import APIRouter, Depends
from sqlalchemy.orm import Session
from sqlalchemy import func
from app.database import get_db
from app import models

router = APIRouter(prefix="/api/ratings", tags=["ratings"])


@router.get("/{hackathon_id}")
def get_rating(hackathon_id: int, db: Session = Depends(get_db)):
    rows = (
        db.query(
            models.Team.id.label("team_id"),
            models.Team.name.label("team_name"),
            models.Project.title.label("project_title"),
            func.coalesce(
                func.sum(models.Score.value * models.Criterion.weight), 0
            ).label("total"),
            func.count(func.distinct(models.Score.jury_id)).label("jury_count"),
        )
        .outerjoin(models.Project, models.Project.team_id == models.Team.id)
        .outerjoin(models.Score, models.Score.project_id == models.Project.id)
        .outerjoin(models.Criterion, models.Criterion.id == models.Score.criterion_id)
        .filter(models.Team.hackathon_id == hackathon_id)
        .group_by(models.Team.id, models.Team.name, models.Project.title)
        .order_by(
            func.coalesce(
                func.sum(models.Score.value * models.Criterion.weight), 0
            ).desc()
        )
        .all()
    )

    result = []
    for i, r in enumerate(rows, start=1):
        result.append({
            "position": i,
            "teamId": r.team_id,
            "teamName": r.team_name,
            "projectTitle": r.project_title,
            "totalScore": float(r.total),
            "juryCount": int(r.jury_count),
        })
    return result