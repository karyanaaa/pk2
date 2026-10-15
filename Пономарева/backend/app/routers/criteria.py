from fastapi import APIRouter, Depends
from sqlalchemy.orm import Session
from app.database import get_db
from app import models, schemas
from app.deps import require_role

router = APIRouter(prefix="/api/criteria", tags=["criteria"])


@router.get("/{hackathon_id}")
def list_criteria(hackathon_id: int, db: Session = Depends(get_db)):
    items = db.query(models.Criterion).filter_by(hackathon_id=hackathon_id).all()
    return [
        {
            "id": c.id,
            "hackathonId": c.hackathon_id,
            "name": c.name,
            "description": c.description,
            "maxScore": c.max_score,
            "weight": float(c.weight),
        }
        for c in items
    ]


@router.post("")
def create_criterion(
    payload: schemas.CriterionIn,
    user: models.User = Depends(require_role("organizer", "admin")),
    db: Session = Depends(get_db),
):
    h = db.query(models.Hackathon).get(payload.hackathonId)
    if not h or (h.organizer_id != user.id and user.role != "admin"):
        return schemas.fail("Хакатон не найден или нет прав")

    existing = db.query(models.Criterion).filter_by(
        hackathon_id=payload.hackathonId,
        name=payload.name
    ).first()
    if existing:
        return schemas.fail("Критерий с таким названием уже существует в этом хакатоне")

    c = models.Criterion(
        hackathon_id=payload.hackathonId,
        name=payload.name,
        description=payload.description,
        max_score=payload.maxScore,
        weight=payload.weight,
    )
    db.add(c)
    db.commit()
    db.refresh(c)
    return schemas.ok(
        {
            "id": c.id,
            "hackathonId": c.hackathon_id,
            "name": c.name,
            "description": c.description,
            "maxScore": c.max_score,
            "weight": float(c.weight),
        },
        "Критерий добавлен",
    )


@router.delete("/{criterion_id}")
def delete_criterion(
    criterion_id: int,
    user: models.User = Depends(require_role("organizer", "admin")),
    db: Session = Depends(get_db),
):
    c = db.query(models.Criterion).get(criterion_id)
    if not c:
        return schemas.fail("Критерий не найден")

    h = db.query(models.Hackathon).get(c.hackathon_id)
    if not h or (h.organizer_id != user.id and user.role != "admin"):
        return schemas.fail("Нет прав на удаление этого критерия")

    db.delete(c)
    db.commit()
    return schemas.ok(None, "Критерий удалён")