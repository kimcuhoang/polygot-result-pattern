import { ICommand } from "@nestjs/cqrs";
import { CreateUserPayload } from "./create-user.payload";

export class CreateUserCommand implements ICommand {
    constructor(public readonly payload: CreateUserPayload) {}
}