from pydantic_settings import BaseSettings


class Settings(BaseSettings):
    DATABASE_URL: str = "postgresql+psycopg://postgres:postgres@localhost:5432/hackathonhub"
    SECRET_KEY: str = "change-me-to-a-long-random-string-in-production"
    ALGORITHM: str = "HS256"
    ACCESS_TOKEN_EXPIRE_MINUTES: int = 1440

    class Config:
        env_file = ".env"


settings = Settings()