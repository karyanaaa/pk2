from pydantic import BaseModel, EmailStr, Field
from typing import Optional, List, Generic, TypeVar
from datetime import datetime

T = TypeVar("T")


class RegisterRequest(BaseModel):
    fullName: str
    email: EmailStr
    password: str = Field(min_length=8)


class LoginRequest(BaseModel):
    email: EmailStr
    password: str


class ForgotPasswordRequest(BaseModel):
    email: EmailStr


class ResetPasswordRequest(BaseModel):
    token: str
    newPassword: str = Field(min_length=8)


class UserOut(BaseModel):
    id: int
    fullName: str = Field(..., alias="full_name")
    email: str
    role: str
    skills: Optional[str] = None
    phone: Optional[str] = None
    avatarUrl: Optional[str] = Field(None, alias="avatar_url")
    createdAt: datetime = Field(..., alias="created_at")
    isActive: bool = Field(..., alias="is_active")

    class Config:
        from_attributes = True
        populate_by_name = True


class LoginResponse(BaseModel):
    token: str
    user: UserOut


class HackathonIn(BaseModel):
    title: str
    description: str
    startDate: datetime
    endDate: datetime
    registrationDeadline: datetime
    rules: Optional[str] = None
    prizes: Optional[str] = None
    taskDescription: Optional[str] = None
    maxTeams: int = 50
    status: str = "open"


class HackathonOut(BaseModel):
    id: int
    organizerId: int = Field(..., alias="organizer_id")
    title: str
    description: str
    startDate: datetime = Field(..., alias="start_date")
    endDate: datetime = Field(..., alias="end_date")
    registrationDeadline: datetime = Field(..., alias="registration_deadline")
    submissionDeadline: Optional[datetime] = Field(None, alias="submission_deadline")
    status: str
    rules: Optional[str] = None
    prizes: Optional[str] = None
    taskDescription: Optional[str] = Field(None, alias="task_description")
    maxTeams: int = Field(..., alias="max_teams")
    organizerName: Optional[str] = None

    class Config:
        from_attributes = True
        populate_by_name = True


class TeamIn(BaseModel):
    hackathonId: int
    name: str
    description: Optional[str] = None


class TeamOut(BaseModel):
    id: int
    hackathonId: int = Field(..., alias="hackathon_id")
    captainId: int = Field(..., alias="captain_id")
    name: str
    description: Optional[str] = None
    createdAt: datetime = Field(..., alias="created_at")
    hackathonTitle: Optional[str] = None
    captainName: Optional[str] = None
    members: List[UserOut] = []

    class Config:
        from_attributes = True
        populate_by_name = True


class ProjectIn(BaseModel):
    teamId: int
    title: str
    description: Optional[str] = None
    repositoryUrl: Optional[str] = None
    filePath: Optional[str] = None
    presentationPath: Optional[str] = None


class ProjectOut(BaseModel):
    id: int
    teamId: int = Field(..., alias="team_id")
    hackathonId: Optional[int] = Field(None, alias="hackathon_id")
    hackathonTitle: Optional[str] = Field(None, alias="hackathon_title")
    title: str
    description: Optional[str] = None
    repositoryUrl: Optional[str] = Field(None, alias="repository_url")
    filePath: Optional[str] = Field(None, alias="file_path")
    presentationPath: Optional[str] = Field(None, alias="presentation_path")
    uploadedAt: datetime = Field(..., alias="uploaded_at")
    teamName: Optional[str] = None

    class Config:
        from_attributes = True
        populate_by_name = True


class CriterionIn(BaseModel):
    hackathonId: int
    name: str
    description: Optional[str] = None
    maxScore: int = 10
    weight: float = 1.0


class CriterionOut(BaseModel):
    id: int
    hackathonId: int = Field(..., alias="hackathon_id")
    name: str
    description: Optional[str] = None
    maxScore: int = Field(..., alias="max_score")
    weight: float

    class Config:
        from_attributes = True
        populate_by_name = True


class ScoreIn(BaseModel):
    projectId: int
    criterionId: int
    value: int
    comment: Optional[str] = None


class ScoreOut(BaseModel):
    id: int
    projectId: int = Field(..., alias="project_id")
    juryId: int = Field(..., alias="jury_id")
    criterionId: int = Field(..., alias="criterion_id")
    value: int
    comment: Optional[str] = None
    scoredAt: datetime = Field(..., alias="scored_at")
    juryName: Optional[str] = None
    criterionName: Optional[str] = None
    projectTitle: Optional[str] = None

    class Config:
        from_attributes = True
        populate_by_name = True


class RatingOut(BaseModel):
    position: int
    teamId: int
    teamName: str
    projectTitle: Optional[str] = None
    totalScore: float
    juryCount: int


class InvitationOut(BaseModel):
    id: int
    teamId: int
    teamName: str
    hackathonTitle: Optional[str] = None
    fromUserName: Optional[str] = None
    status: str
    createdAt: datetime


class ChatMessageIn(BaseModel):
    text: str


class ChatMessageOut(BaseModel):
    id: int
    userId: int
    userName: str
    text: str
    createdAt: datetime


class RoleChangeRequest(BaseModel):
    role: str


class ApiResponse(BaseModel, Generic[T]):
    success: bool = True
    message: Optional[str] = None
    data: Optional[T] = None


def ok(data=None, message=None):
    return {"success": True, "message": message, "data": data}


def fail(message: str, data=None):
    return {"success": False, "message": message, "data": data}