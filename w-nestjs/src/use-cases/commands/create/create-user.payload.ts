import { ApiProperty } from "@nestjs/swagger";
import { IsEmail, IsNotEmpty, IsString } from "class-validator";

export class CreateUserPayload {
    @ApiProperty({ required: true })
    @IsString()
    @IsNotEmpty()
    @IsEmail()
    email: string;
}