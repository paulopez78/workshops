DROP DATABASE "meetup";
CREATE DATABASE "meetup";

CREATE SCHEMA scheduling;
CREATE USER meetup WITH PASSWORD 'password';

GRANT ALL PRIVILEGES ON DATABASE "meetup" to meetup;
GRANT ALL PRIVILEGES ON SCHEMA "scheduling" to meetup;

SELECT M."Id", M."Title", M."Group", AL."Capacity", M."Status", M."Version", AL."Version",A."Id", A."UserId", A."Status" , A."ModifiedAt"
FROM "MeetupEvent" M 
LEFT JOIN "AttendantList" AL on M."Id" = AL."Id" 
LEFT JOIN "Attendant" A on AL."Id" = A."AttendantListAggregateId";


DELETE FROM "AttendantList";
DELETE FROM "MeetupEvent";
