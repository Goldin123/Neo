CREATE TABLE PostTags (
    PostId INT NOT NULL,
    TagId INT NOT NULL,
    PRIMARY KEY (PostId, TagId),
    FOREIGN KEY (PostId) REFERENCES Posts(Id),
    FOREIGN KEY (TagId) REFERENCES Tags(Id)
);