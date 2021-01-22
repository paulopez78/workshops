DROP DATABASE "meetup";
DROP USER "meetup";

CREATE DATABASE "meetup";
CREATE SCHEMA group_management;

CREATE USER meetup WITH PASSWORD 'password';

GRANT ALL PRIVILEGES ON DATABASE "meetup" to meetup;
GRANT ALL PRIVILEGES ON SCHEMA "group_management" to meetup;

SELECT * FROM meetup.group_management."MeetupGroups";
SELECT * FROM meetup.group_management."Members";

SELECT G."Id", G."Title", G."Slug", G."Description", G."Location", M."Id", M."UserId",  M."JoinedAt"
FROM meetup.group_management."MeetupGroups" G  LEFT JOIN meetup.group_management."Members" M on M."GroupId" = G."Id"
WHERE G."Slug" = 'netcorebcn';
