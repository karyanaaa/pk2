from fastapi import APIRouter, Depends
from sqlalchemy.orm import Session
from datetime import datetime
from app.database import get_db
from app import models, schemas
from app.deps import get_current_user, require_role

router = APIRouter(prefix="/api/scores", tags=["scores"])


def _is_jury_assigned(db, hackathon_id: int, user_id: int) -> bool:
    return db.query(models.HackathonJury).filter_by(
        hackathon_id=hackathon_id, jury_id=user_id
    ).first() is not None


@router.post("")
def submit_score(
    payload: schemas.ScoreIn,
    user: models.User = Depends(require_role("jury", "admin")),
    db: Session = Depends(get_db),
):
    project = db.query(models.Project).get(payload.projectId)
    criterion = db.query(models.Criterion).get(payload.criterionId)
    if not project or not criterion:
        return schemas.fail("Проект или критерий не найден")

    if criterion.hackathon_id != project.team.hackathon_id:
        return schemas.fail("Критерий не относится к хакатону этого проекта")

    hackathon = project.team.hackathon

    if user.role != "admin":
        if not _is_jury_assigned(db, hackathon.id, user.id):
            return schemas.fail("Вы не назначены жюри на этот хакатон")

    deadline = hackathon.submission_deadline or hackathon.end_date
    if datetime.now() > deadline:
        return schemas.fail("Срок приёма оценок истёк")

    if datetime.now() > hackathon.end_date:
        return schemas.fail("Хакатон завершён, оценки больше не принимаются")

    existing = db.query(models.Score).filter_by(
        project_id=payload.projectId,
        jury_id=user.id,
        criterion_id=payload.criterionId,
    ).first()

    if existing:
        existing.value = payload.value
        existing.comment = payload.comment
        existing.scored_at = datetime.now()
        db.commit()
        db.refresh(existing)
        return schemas.ok({"id": existing.id}, "Оценка обновлена")

    s = models.Score(
        project_id=payload.projectId,
        jury_id=user.id,
        criterion_id=payload.criterionId,
        value=payload.value,
        comment=payload.comment,
    )
    db.add(s)
    db.commit()
    db.refresh(s)
    return schemas.ok({"id": s.id}, "Оценка выставлена")


@router.get("/my")
def my_scores(
    user: models.User = Depends(get_current_user),
    db: Session = Depends(get_db),
):
    team_ids = [
        m.team_id
        for m in db.query(models.TeamMember).filter(models.TeamMember.user_id == user.id).all()
    ]
    if not team_ids:
        return []

    project_ids = [
        p.id
        for p in db.query(models.Project).filter(models.Project.team_id.in_(team_ids)).all()
    ]
    if not project_ids:
        return []

    scores = (
        db.query(models.Score)
        .filter(models.Score.project_id.in_(project_ids))
        .order_by(models.Score.scored_at.desc())
        .all()
    )

    result = []
    for s in scores:
        result.append({
            "id": s.id,
            "projectId": s.project_id,
            "projectTitle": s.project.title if s.project else None,
            "juryName": s.jury.full_name if s.jury else None,
            "criterionName": s.criterion.name if s.criterion else None,
            "value": s.value,
            "comment": s.comment,
            "scoredAt": s.scored_at,
        })
    return result