import { CommandHandler, ICommandHandler } from "@nestjs/cqrs";
import { Reason, Result } from "src/models";
import { CreateUserCommand } from "./create-user.command";

@CommandHandler(CreateUserCommand)
export class CreateUserHandler implements ICommandHandler<CreateUserCommand, Result> {
    public async execute(command: CreateUserCommand): Promise<Result<unknown>> {
        const { payload } = command;

        if (payload.email === "not-accept@email.com") {
            return Result.Failure(Reason.NotAcceptable());
        }

        if (payload.email === "duplicate@email.com") {
            return Result.Failure(Reason.Conflict());
        }

        if (payload.email === "exception@email.com") {
            throw new Error("Unexpected error occurred while creating user.");
        }

        return Result.Success();
    }
}