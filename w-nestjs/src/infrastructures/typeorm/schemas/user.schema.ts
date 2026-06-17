import { User } from "../../../domains/user";
import { EntitySchema, EntitySchemaColumnOptions } from "typeorm";
import { snakeCase } from "typeorm/util/StringUtils.js";


export const UserSchema = new EntitySchema<User>({
    name: User.name,
    target: User,
    tableName: snakeCase(User.name),
    columns: {
        id: {
            type: String,
            primary: true,
            nullable: false,
            length: 26 // ULID length
        } as EntitySchemaColumnOptions,

        name: {
            type: String,
            nullable: false,
        } as EntitySchemaColumnOptions,

        email: {
            type: String,
            nullable: false,
            unique: true,
        } as EntitySchemaColumnOptions,
    }
});