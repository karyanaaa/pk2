from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
from fastapi.staticfiles import StaticFiles   # ← новое
import os   
from app.database import Base, engine
from app.routers import (
    auth, users, hackathons, teams, projects,
    criteria, scores, ratings, admin, stats, uploads,
    invitations, chat
)

Base.metadata.create_all(bind=engine)

app = FastAPI(title="HackathonHub API", version="1.0.0")

app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_methods=["*"],
    allow_headers=["*"],
)
# === Раздача загруженных файлов ===
os.makedirs("uploads/avatars", exist_ok=True)
app.mount("/uploads", StaticFiles(directory="uploads"), name="uploads")

app.include_router(auth.router)
app.include_router(users.router)
app.include_router(hackathons.router)
app.include_router(teams.router)
app.include_router(projects.router)
app.include_router(criteria.router)
app.include_router(scores.router)
app.include_router(ratings.router)
app.include_router(admin.router)
app.include_router(stats.router)
app.include_router(uploads.router)
app.include_router(invitations.router)
app.include_router(chat.router)


@app.get("/")
def root():
    return {"status": "ok", "service": "HackathonHub API"}