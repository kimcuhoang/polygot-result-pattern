import { IQuery } from "@nestjs/cqrs";
import { ApiProperty } from "@nestjs/swagger";
import { IsEmail, IsNotEmpty } from "class-validator";


export class FindUserCriteria {
    @ApiProperty({ description: "The unique identifier of the user, which can be either an email or a user ID." })
    @IsNotEmpty()
    @IsEmail()
    email: string;
}

export class FindUserQuery implements IQuery {
    constructor(public readonly criteria: FindUserCriteria) { }
}