from fastapi import APIRouter, Depends, HTTPException
from sqlalchemy.orm import Session
from datetime import datetime
from typing import Optional
from app.database import get_db
from app import models, schemas
from app.deps import require_role, get_current_user

router = APIRouter(prefix="/api/hackathons", tags=["hackathons"])


def _compute_status(h: models.Hackathon) -> str:
    now = datetime.now()
    if now > h.end_date:
        return "finished"
    if now < h.start_date:
        if now > h.registration_deadline:
            return "closed"
        return "open"
    return "ongoing"


def _to_out(h: models.Hackathon) -> dict:
    return {
        "id": h.id,
        "organizerId": h.organizer_id,
        "title": h.title,
        "description": h.description,
        "startDate": h.start_date,
        "endDate": h.end_date,
        "registrationDeadline": h.registration_deadline,
        "submissionDeadline": h.submission_deadline,
        "status": _compute_status(h),
        "rules": h.rules,
        "prizes": h.prizes,
        "taskDescription": h.task_description,
        "maxTeams": h.max_teams,
        "organizerName": h.organizer.full_name if h.organizer else None,
    }


@router.get("")
def list_hackathons(
    status: Optional[str] = None,
    search: Optional[str] = None,
    sort: Optional[str] = "date_desc",
    db: Session = Depends(get_db),
):
    q = db.query(models.Hackathon)
    if search:
        q = q.filter(models.Hackathon.title.ilike(f"%{search}%"))
    items = q.all()

    if status and status != "all":
        items = [h for h in items if _compute_status(h) == status]

    if sort == "date_asc":
        items.sort(key=lambda h: h.start_date)
    elif sort == "date_desc":
        items.sort(key=lambda h: h.start_date, reverse=True)
    elif sort == "title":
        items.sort(key=lambda h: (h.title or "").lower())
    elif sort == "deadline":
        items.sort(key=lambda h: h.registration_deadline)

    return [_to_out(h) for h in items]


@router.get("/{hid}")
def get_hackathon(hid: int, db: Session = Depends(get_db)):
    h = db.query(models.Hackathon).get(hid)
    if not h:
        raise HTTPException(404, "Хакатон не найден")
    return _to_out(h)


@router.post("")
def create_hackathon(
    payload: schemas.HackathonIn,
    user: models.User = Depends(require_role("organizer", "admin")),
    db: Session = Depends(get_db),
):
    if payload.startDate >= payload.endDate:
        return schemas.fail("Дата старта должна быть раньше даты окончания")
    if payload.registrationDeadline > payload.startDate:
        return schemas.fail("Дедлайн регистрации должен быть до старта")

    h = models.Hackathon(
        organizer_id=user.id,
        title=payload.title,
        description=payload.description,
        start_date=payload.startDate,
        end_date=payload.endDate,
        registration_deadline=payload.registrationDeadline,
        submission_deadline=payload.endDate,
        rules=payload.rules,
        prizes=payload.prizes,
        task_description=payload.taskDescription,
        max_teams=payload.maxTeams,
        status="open",
    )
    db.add(h)
    db.commit()
    db.refresh(h)
    return schemas.ok(_to_out(h), "Хакатон создан")


@router.put("/{hid}")
def update_hackathon(
    hid: int,
    payload: schemas.HackathonIn,
    user: models.User = Depends(require_role("organizer", "admin")),
    db: Session = Depends(get_db),
):
    h = db.query(models.Hackathon).get(hid)
    if not h:
        return schemas.fail("Хакатон не найден")
    if h.organizer_id != user.id and user.role != "admin":
        return schemas.fail("Вы не являетесь организатором этого хакатона")

    computed = _compute_status(h)
    if computed in ("ongoing", "finished") and user.role != "admin":
        h.description = payload.description
        h.rules = payload.rules
        h.prizes = payload.prizes
        h.task_description = payload.taskDescription
        db.commit()
        db.refresh(h)
        return schemas.ok(
            _to_out(h),
            "Хакатон уже идёт — можно менять только описание, правила, призы и задание"
        )

    h.title = payload.title
    h.description = payload.description
    h.start_date = payload.startDate
    h.end_date = payload.endDate
    h.registration_deadline = payload.registrationDeadline
    h.submission_deadline = payload.endDate
    h.rules = payload.rules
    h.prizes = payload.prizes
    h.task_description = payload.taskDescription
    h.max_teams = payload.maxTeams
    h.status = payload.status
    db.commit()
    db.refresh(h)
    return schemas.ok(_to_out(h), "Хакатон обновлён")


@router.delete("/{hid}")
def delete_hackathon(
    hid: int,
    user: models.User = Depends(require_role("organizer", "admin")),
    db: Session = Depends(get_db),
):
    h = db.query(models.Hackathon).get(hid)
    if not h:
        return schemas.fail("Хакатон не найден")
    if h.organizer_id != user.id and user.role != "admin":
        return schemas.fail("Вы не являетесь организатором этого хакатона")
    db.delete(h)
    db.commit()
    return schemas.ok(None, "Хакатон удалён")


# ============= РЕГИСТРАЦИЯ =============

@router.post("/{hid}/register")
def register_for_hackathon(
    hid: int,
    user: models.User = Depends(get_current_user),
    db: Session = Depends(get_db),
):
    h = db.query(models.Hackathon).get(hid)
    if not h:
        return schemas.fail("Хакатон не найден")

    if user.role != "participant":
        return schemas.fail("Только участники могут регистрироваться")

    if datetime.now() > h.registration_deadline:
        return schemas.fail("Регистрация закрыта")

    existing = db.query(models.HackathonRegistration).filter_by(
        hackathon_id=hid, user_id=user.id
    ).first()
    if existing:
        return schemas.fail("Вы уже подали заявку")

    reg = models.HackathonRegistration(
        hackathon_id=hid, user_id=user.id, status="pending"
    )
    db.add(reg)
    db.commit()
    return schemas.ok(None, "Заявка отправлена организатору")


@router.get("/{hid}/my-registration")
def my_registration(
    hid: int,
    user: models.User = Depends(get_current_user),
    db: Session = Depends(get_db),
):
    r = db.query(models.HackathonRegistration).filter_by(
        hackathon_id=hid, user_id=user.id
    ).first()
    if not r:
        return {"status": None}
    return {"status": r.status, "id": r.id}


@router.get("/{hid}/registrations")
def list_registrations(
    hid: int,
    user: models.User = Depends(require_role("organizer", "admin")),
    db: Session = Depends(get_db),
):
    h = db.query(models.Hackathon).get(hid)
    if not h:
        return schemas.fail("Хакатон не найден")
    if h.organizer_id != user.id and user.role != "admin":
        return schemas.fail("Нет прав")

    items = db.query(models.HackathonRegistration).filter_by(hackathon_id=hid).all()
    return [
        {
            "id": r.id,
            "userId": r.user_id,
            "fullName": r.user.full_name if r.user else None,
            "email": r.user.email if r.user else None,
            "status": r.status,
            "createdAt": r.created_at,
        }
        for r in items
    ]


@router.post("/registrations/{reg_id}/approve")
def approve_registration(
    reg_id: int,
    user: models.User = Depends(require_role("organizer", "admin")),
    db: Session = Depends(get_db),
):
    r = db.query(models.HackathonRegistration).get(reg_id)
    if not r:
        return schemas.fail("Заявка не найдена")
    h = db.query(models.Hackathon).get(r.hackathon_id)
    if h.organizer_id != user.id and user.role != "admin":
        return schemas.fail("Нет прав")

    r.status = "approved"
    db.commit()
    return schemas.ok(None, "Заявка одобрена")


@router.post("/registrations/{reg_id}/reject")
def reject_registration(
    reg_id: int,
    user: models.User = Depends(require_role("organizer", "admin")),
    db: Session = Depends(get_db),
):
    r = db.query(models.HackathonRegistration).get(reg_id)
    if not r:
        return schemas.fail("Заявка не найдена")
    h = db.query(models.Hackathon).get(r.hackathon_id)
    if h.organizer_id != user.id and user.role != "admin":
        return schemas.fail("Нет прав")

    r.status = "rejected"
    db.commit()
    return schemas.ok(None, "Заявка отклонена")


# ============= ЖЮРИ =============

@router.get("/{hid}/jury")
def list_jury(hid: int, db: Session = Depends(get_db)):
    items = db.query(models.HackathonJury).filter_by(hackathon_id=hid).all()
    return [
        {
            "id": j.id,
            "userId": j.jury_id,
            "fullName": j.jury.full_name if j.jury else None,
            "email": j.jury.email if j.jury else None,
        }
        for j in items
    ]


@router.post("/{hid}/jury/{user_id}")
def assign_jury(
    hid: int,
    user_id: int,
    user: models.User = Depends(require_role("organizer", "admin")),
    db: Session = Depends(get_db),
):
    h = db.query(models.Hackathon).get(hid)
    if not h:
        return schemas.fail("Хакатон не найден")
    if h.organizer_id != user.id and user.role != "admin":
        return schemas.fail("Нет прав")

    target = db.query(models.User).get(user_id)
    if not target or target.role != "jury":
        return schemas.fail("Пользователь не является жюри")

    existing = db.query(models.HackathonJury).filter_by(
        hackathon_id=hid, jury_id=user_id
    ).first()
    if existing:
        return schemas.fail("Жюри уже назначено")

    db.add(models.HackathonJury(hackathon_id=hid, jury_id=user_id))
    db.commit()
    return schemas.ok(None, "Жюри назначено")


@router.delete("/{hid}/jury/{user_id}")
def remove_jury(
    hid: int,
    user_id: int,
    user: models.User = Depends(require_role("organizer", "admin")),
    db: Session = Depends(get_db),
):
    h = db.query(models.Hackathon).get(hid)
    if not h:
        return schemas.fail("Хакатон не найден")
    if h.organizer_id != user.id and user.role != "admin":
        return schemas.fail("Нет прав")

    j = db.query(models.HackathonJury).filter_by(
        hackathon_id=hid, jury_id=user_id
    ).first()
    if not j:
        return schemas.fail("Жюри не назначено")

    db.delete(j)
    db.commit()
    return schemas.ok(None, "Жюри снято")


# ============= OVERVIEW =============

@router.get("/my/teams-overview")
def my_teams_overview(
    user: models.User = Depends(require_role("organizer", "admin")),
    db: Session = Depends(get_db),
):
    if user.role == "admin":
        hackathons = db.query(models.Hackathon).all()
    else:
        hackathons = db.query(models.Hackathon).filter(
            models.Hackathon.organizer_id == user.id
        ).all()

    result = []
    for h in hackathons:
        for t in h.teams:
            proj = t.project
            deadline = h.submission_deadline or h.end_date
            now = datetime.now()
            if proj is not None:
                submission_status = "submitted"
                hours_left = None
            elif now > deadline:
                submission_status = "missed"
                hours_left = 0
            else:
                submission_status = "pending"
                hours_left = int((deadline - now).total_seconds() // 3600)

            result.append({
                "teamId": t.id,
                "teamName": t.name,
                "hackathonId": h.id,
                "hackathonTitle": h.title,
                "captainName": next(
                    (m.user.full_name for m in t.members if m.user_id == t.captain_id),
                    None,
                ),
                "membersCount": len(t.members),
                "members": [
                    {"id": m.user.id, "fullName": m.user.full_name, "email": m.user.email}
                    for m in t.members
                ],
                "submissionStatus": submission_status,
                "hoursLeft": hours_left,
            })
    return result