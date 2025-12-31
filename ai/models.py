from pydantic import BaseModel, Field

class BatchEmbedRequest(BaseModel):
    texts: list[str]

class UserVectorRequest(BaseModel):
    subjects: list[str] = []       
    subjectWeights: list[float] = []
    tags: list[str] = []           
    tagWeights: list[float] = []

class ConvVectorRequest(BaseModel):
    name: str                      
    subject: str | None = None      
    tags: list[str] = []

class ConvVectorDto(BaseModel):
    Id: str = Field(alias="id")
    Vector: list[float] = Field(alias="vector")

    class Config:
        populate_by_name = True

class RecommendRequest(BaseModel):
    UserVector: list[float] = Field(alias="userVector")
    ConversationVectors: list[dict] = Field(alias="conversationVectors")
    TopK: int = Field(default=10, alias="topK")
    MinSimilarity: float = Field(default=0.3, alias="minSimilarity")

    class Config:
        populate_by_name = True

class SimilarUsersRequest(BaseModel):
    ConvVector: list[float] = Field(alias="convVector")
    UserVectors: list[dict] = Field(alias="userVectors")
    TopK: int = Field(default=10, alias="topK")
    MinScore: float = Field(default=0.3, alias="minScore")

    class Config:
        populate_by_name = True

class ClusterUsersRequest(BaseModel):
    UserVectors: list[dict] = Field(alias="userVectors")
    MinClusterSize: int = Field(default=5, alias="minClusterSize")

    class Config:
        populate_by_name = True