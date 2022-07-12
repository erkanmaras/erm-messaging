##### SagaActionLog #####

create table if not exists _SagaActionLog
(
    Id          bigint unsigned auto_increment primary key,
    SagaId      binary(16)                             not null,
    MessageName varchar(500) charset utf8              not null,
    Envelope    json                                   not null,
    CreatedAt   datetime(3) default (UTC_TIMESTAMP(3)) not null,
    constraint Id unique (Id)
);

##### SagaState #####

create table if not exists _SagaState
(
    Id         bigint unsigned auto_increment primary key,
    SagaId     binary(16)                             not null,
    SagaType   varchar(500) charset utf8              not null,
    SagaStatus int                                    not null,
    Data       json                                   null,
    CreatedAt  datetime(3) default (UTC_TIMESTAMP(3)) not null,
    RowVersion int                                    not null,
    constraint Id unique (Id),
    constraint SagaState_SagaId_SagaType_uindex unique (SagaType, SagaId)
);

