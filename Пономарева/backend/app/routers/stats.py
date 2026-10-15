from fastapi import APIRouter, Depends
from sqlalchemy.orm import Session
from app.database import get_db
from app import models
from app.deps import require_role

router = APIRouter(prefix="/api/stats", tags=["stats"])


@router.get("/hackathon/{hid}")
def hackathon_stats(
    hid: int,
    user: models.User = Depends(require_role("organizer", "admin")),
    db: Session = Depends(get_db),
):
    teams_count = db.query(models.Team).filter(models.Team.hackathon_id == hid).count()

    projects_count = (
        db.query(models.Project)
        .join(models.Team, models.Team.id == models.Project.team_id)
        .filter(models.Team.hackathon_id == hid)
        .count()
    )

    members_count = (
        db.query(models.TeamMember)
        .join(models.Team, models.Team.id == models.TeamMember.team_id)
        .filter(models.Team.hackathon_id == hid)
        .count()
    )

    return {
        "teamsCount": teams_count,
        "projectsCount": projects_count,
        "membersCount": members_count,
        "notSubmitted": teams_count - projects_count,
    }