from sqlalchemy import (
    Column, Integer, String, Text, DateTime, Boolean,
    ForeignKey, Numeric, CheckConstraint, UniqueConstraint
)
from sqlalchemy.orm import relationship
from sqlalchemy.sql import func
from app.database import Base


class User(Base):
    __tablename__ = "users"

    id = Column(Integer, primary_key=True, index=True)
    full_name = Column(String(100), nullable=False)
    email = Column(String(120), unique=True, nullable=False, index=True)
    password_hash = Column(String(255), nullable=False)
    role = Column(String(20), nullable=False, default="participant")
    skills = Column(String(500), nullable=True)
    phone = Column(String(20), nullable=True)
    avatar_url = Column(String(500), nullable=True)
    created_at = Column(DateTime(timezone=True), server_default=func.now())
    is_active = Column(Boolean, default=True)
    reset_token = Column(String(255), nullable=True)
    reset_token_expiry = Column(DateTime(timezone=True), nullable=True)

    __table_args__ = (
        CheckConstraint(
            "role IN ('admin','organizer','participant','jury')",
            name="ck_user_role"
        ),
    )

    organized_hackathons = relationship("Hackathon", back_populates="organizer")
    teams = relationship("TeamMember", back_populates="user")
    scores = relationship("Score", back_populates="jury")
    jury_assignments = relationship("HackathonJury", back_populates="jury", cascade="all, delete-orphan")


class Hackathon(Base):
    __tablename__ = "hackathons"

    id = Column(Integer, primary_key=True, index=True)
    organizer_id = Column(Integer, ForeignKey("users.id", ondelete="CASCADE"), nullable=False)
    title = Column(String(200), nullable=False)
    description = Column(Text, nullable=False)
    start_date = Column(DateTime, nullable=False)
    end_date = Column(DateTime, nullable=False)
    registration_deadline = Column(DateTime, nullable=False)
    submission_deadline = Column(DateTime, nullable=True)
    status = Column(String(20), default="open")
    rules = Column(Text, nullable=True)
    prizes = Column(Text, nullable=True)
    task_description = Column(Text, nullable=True)
    max_teams = Column(Integer, default=50)

    organizer = relationship("User", back_populates="organized_hackathons")
    teams = relationship("Team", back_populates="hackathon", cascade="all, delete-orphan")
    criteria = relationship("Criterion", back_populates="hackathon", cascade="all, delete-orphan")
    chat_messages = relationship("ChatMessage", back_populates="hackathon", cascade="all, delete-orphan")
    jury_assignments = relationship("HackathonJury", back_populates="hackathon", cascade="all, delete-orphan")
    registrations = relationship("HackathonRegistration", back_populates="hackathon", cascade="all, delete-orphan")


class Team(Base):
    __tablename__ = "teams"

    id = Column(Integer, primary_key=True, index=True)
    hackathon_id = Column(Integer, ForeignKey("hackathons.id", ondelete="CASCADE"), nullable=False)
    captain_id = Column(Integer, ForeignKey("users.id"), nullable=False)
    name = Column(String(100), nullable=False)
    description = Column(Text, nullable=True)
    created_at = Column(DateTime(timezone=True), server_default=func.now())

    hackathon = relationship("Hackathon", back_populates="teams")
    members = relationship("TeamMember", back_populates="team", cascade="all, delete-orphan")
    project = relationship("Project", back_populates="team", uselist=False, cascade="all, delete-orphan")

    __table_args__ = (
        UniqueConstraint("hackathon_id", "name", name="uq_team_name_per_hackathon"),
    )


class TeamMember(Base):
    __tablename__ = "team_members"

    id = Column(Integer, primary_key=True, index=True)
    team_id = Column(Integer, ForeignKey("teams.id", ondelete="CASCADE"), nullable=False)
    user_id = Column(Integer, ForeignKey("users.id", ondelete="CASCADE"), nullable=False)
    joined_at = Column(DateTime(timezone=True), server_default=func.now())

    team = relationship("Team", back_populates="members")
    user = relationship("User", back_populates="teams")

    __table_args__ = (
        UniqueConstraint("team_id", "user_id", name="uq_team_user"),
    )


class Project(Base):
    __tablename__ = "projects"

    id = Column(Integer, primary_key=True, index=True)
    team_id = Column(Integer, ForeignKey("teams.id", ondelete="CASCADE"), unique=True, nullable=False)
    title = Column(String(200), nullable=False)
    description = Column(Text, nullable=True)
    repository_url = Column(String(500), nullable=True)
    file_path = Column(String(500), nullable=True)
    presentation_path = Column(String(500), nullable=True)
    uploaded_at = Column(DateTime(timezone=True), server_default=func.now())

    team = relationship("Team", back_populates="project")
    scores = relationship("Score", back_populates="project", cascade="all, delete-orphan")


class Criterion(Base):
    __tablename__ = "criteria"

    id = Column(Integer, primary_key=True, index=True)
    hackathon_id = Column(Integer, ForeignKey("hackathons.id", ondelete="CASCADE"), nullable=False)
    name = Column(String(100), nullable=False)
    description = Column(Text, nullable=True)
    max_score = Column(Integer, default=10)
    weight = Column(Numeric(5, 2), default=1.0)

    hackathon = relationship("Hackathon", back_populates="criteria")
    scores = relationship("Score", back_populates="criterion", cascade="all, delete-orphan")

    __table_args__ = (
        UniqueConstraint("hackathon_id", "name", name="uq_criterion_name_per_hackathon"),
    )


class Score(Base):
    __tablename__ = "scores"

    id = Column(Integer, primary_key=True, index=True)
    project_id = Column(Integer, ForeignKey("projects.id", ondelete="CASCADE"), nullable=False)
    jury_id = Column(Integer, ForeignKey("users.id", ondelete="CASCADE"), nullable=False)
    criterion_id = Column(Integer, ForeignKey("criteria.id", ondelete="CASCADE"), nullable=False)
    value = Column(Integer, nullable=False)
    comment = Column(Text, nullable=True)
    scored_at = Column(DateTime(timezone=True), server_default=func.now())

    project = relationship("Project", back_populates="scores")
    jury = relationship("User", back_populates="scores")
    criterion = relationship("Criterion", back_populates="scores")

    __table_args__ = (
        UniqueConstraint("project_id", "jury_id", "criterion_id", name="uq_score"),
        CheckConstraint("value >= 0 AND value <= 100", name="ck_score_value"),
    )


class TeamInvitation(Base):
    __tablename__ = "team_invitations"

    id = Column(Integer, primary_key=True, index=True)
    team_id = Column(Integer, ForeignKey("teams.id", ondelete="CASCADE"), nullable=False)
    user_id = Column(Integer, ForeignKey("users.id", ondelete="CASCADE"), nullable=False)
    status = Column(String(20), default="pending")
    created_at = Column(DateTime(timezone=True), server_default=func.now())

    team = relationship("Team")
    user = relationship("User")

    __table_args__ = (
        UniqueConstraint("team_id", "user_id", name="uq_team_invitation"),
    )


class ChatMessage(Base):
    __tablename__ = "chat_messages"

    id = Column(Integer, primary_key=True, index=True)
    hackathon_id = Column(Integer, ForeignKey("hackathons.id", ondelete="CASCADE"), nullable=False)
    user_id = Column(Integer, ForeignKey("users.id", ondelete="CASCADE"), nullable=False)
    text = Column(Text, nullable=False)
    created_at = Column(DateTime(timezone=True), server_default=func.now())

    hackathon = relationship("Hackathon", back_populates="chat_messages")
    user = relationship("User")


class HackathonJury(Base):
    __tablename__ = "hackathon_jury"

    id = Column(Integer, primary_key=True, index=True)
    hackathon_id = Column(Integer, ForeignKey("hackathons.id", ondelete="CASCADE"), nullable=False)
    jury_id = Column(Integer, ForeignKey("users.id", ondelete="CASCADE"), nullable=False)
    created_at = Column(DateTime(timezone=True), server_default=func.now())

    hackathon = relationship("Hackathon", back_populates="jury_assignments")
    jury = relationship("User", back_populates="jury_assignments")

    __table_args__ = (
        UniqueConstraint("hackathon_id", "jury_id", name="uq_hackathon_jury"),
    )


class HackathonRegistration(Base):
    __tablename__ = "hackathon_registrations"

    id = Column(Integer, primary_key=True, index=True)
    hackathon_id = Column(Integer, ForeignKey("hackathons.id", ondelete="CASCADE"), nullable=False)
    user_id = Column(Integer, ForeignKey("users.id", ondelete="CASCADE"), nullable=False)
    status = Column(String(20), default="pending")
    created_at = Column(DateTime(timezone=True), server_default=func.now())

    hackathon = relationship("Hackathon", back_populates="registrations")
    user = relationship("User")

    __table_args__ = (
        UniqueConstraint("hackathon_id", "user_id", name="uq_hackathon_registration"),
    )