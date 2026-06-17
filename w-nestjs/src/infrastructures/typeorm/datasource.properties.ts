import { SnakeNamingStrategy } from "typeorm-naming-strategies";
import { UserSchema } from "./schemas/user.schema";
import { ConfigService } from "@nestjs/config";
import { PostgresConnectionOptions } from "typeorm/driver/postgres/PostgresConnectionOptions.js";

export const getPostgresConnectionOptions = (configService: ConfigService): PostgresConnectionOptions => ({
    type: 'postgres',
    namingStrategy: new SnakeNamingStrategy(),
    url: configService.get<string>("POSTGRES_DATABASE_URL"),
    logging: configService.get<string>("POSTGRES_LOG_ENABLED")?.toLowerCase() === 'true',
    synchronize: false,
    autoLoadEntities: true,
    dateStrings: ["timestamp without time zone"],
    timezone: "UTC",
    migrationsRun: false,
    migrationsTableName: "migration_histories",
    migrationsTransactionMode: "all",
    migrations: [
        "dist/**/migrations/*.js"
    ],
    entities: [
        UserSchema
    ],
    
} as PostgresConnectionOptions);