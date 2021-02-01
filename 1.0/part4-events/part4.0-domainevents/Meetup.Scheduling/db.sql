DROP DATABASE "meetup";
CREATE DATABASE "meetup";

CREATE SCHEMA scheduling;
CREATE USER meetup WITH PASSWORD 'password';

GRANT ALL PRIVILEGES ON DATABASE "meetup" to meetup;
GRANT ALL PRIVILEGES ON SCHEMA "scheduling" to meetup;


SELECT M."Id", M."Title", M."Description", M."Group", M."Status", M."Start", M."End", M."IsOnline", M."Url", M."Address", M."Version", AL."Capacity", AL."Version",A."Id", A."UserId", A."Status" , A."ModifiedAt"
FROM meetup.scheduling."MeetupEvent" M
         LEFT JOIN meetup.scheduling."AttendantList" AL on M."Id" = AL."Id"
         LEFT JOIN meetup.scheduling."Attendant" A on AL."Id" = A."AttendantListId"


DELETE FROM meetup.scheduling."AttendantList";
DELETE FROM meetup.scheduling."MeetupEvent";