# TODO : CreatedAt ve MessageStatus aslında bu tabloda bulunmayabilir.Fayda zarar analizi yapılmalı.

create table if not exists _MessageStatusRegistry
(
    Id            bigint unsigned auto_increment primary key,
    MessageId     binary(16)                             not null,
    MessageStatus int                                    not null,
    CreatedAt     datetime(3) default (UTC_TIMESTAMP(3)) not null,
    constraint Id unique (Id),
    constraint MessageStatusRegistry_MessageId_uindex
        unique (MessageId)
);

create table if not exists _MessageStatusHistory
(
    Id            bigint unsigned auto_increment primary key,
    MessageId     binary(16)                             not null,
    MessageStatus int                                    not null,
    CreatedAt     datetime(3) default (UTC_TIMESTAMP(3)) not null,
    FaultDetails  json                                   null,
    constraint Id
        unique (Id)
);

DROP PROCEDURE IF EXISTS _MessageStatusMarkAsProcessing;

CREATE PROCEDURE _MessageStatusMarkAsProcessing(
    IN _MessageId BINARY(16),
    IN _MessageStatus integer,
    IN _CreatedAt DATETIME)
BEGIN

    INSERT INTO _MessageStatusRegistry (MessageId, MessageStatus, CreatedAt)
    VALUES (_MessageId, _MessageStatus, _CreatedAt);

    INSERT INTO _MessageStatusHistory (MessageId, MessageStatus, CreatedAt)
    VALUES (_MessageId, _MessageStatus, _CreatedAt);

END;

DROP PROCEDURE IF EXISTS _MessageStatusUpdate;

CREATE PROCEDURE _MessageStatusUpdate(
    IN _MessageId BINARY(16),
    IN _MessageStatus integer,
    IN _CreatedAt DATETIME)
BEGIN

    INSERT INTO _MessageStatusRegistry (MessageId, MessageStatus, CreatedAt)
    VALUES (_MessageId, _MessageStatus, _CreatedAt)
    ON DUPLICATE KEY
        UPDATE MessageStatus=_MessageStatus, CreatedAt=_CreatedAt;

    INSERT INTO _MessageStatusHistory (MessageId, MessageStatus, CreatedAt)
    VALUES (_MessageId, _MessageStatus, _CreatedAt);

END;

DROP PROCEDURE IF EXISTS _MessageStatusMarkAsFaulted;

CREATE PROCEDURE _MessageStatusMarkAsFaulted(
    IN _MessageId BINARY(16),
    IN _MessageStatus INTEGER,
    IN _CreatedAt DATETIME,
    IN _FaultDetails JSON)
BEGIN

    /* MessageStatusRegistry den kayıt silinmeli ki mesaj tekrar işlene bilsin */
    DELETE FROM _MessageStatusRegistry WHERE MessageId = _MessageId;

    INSERT INTO _MessageStatusHistory (MessageId, MessageStatus, CreatedAt, FaultDetails)
    VALUES (_MessageId, _MessageStatus, _CreatedAt, _FaultDetails);

END;

