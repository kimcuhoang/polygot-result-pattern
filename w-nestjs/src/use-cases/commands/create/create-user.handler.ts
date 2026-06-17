import { CommandHandler, ICommandHandler } from "@nestjs/cqrs";
import { Reason, Result } from "src/models";
import { CreateUserCommand } from "./create-user.command";
import { InjectRepository } from "@nestjs/typeorm";
import { User } from "@src/domains/user";
import { Equal, Repository } from "typeorm";

@CommandHandler(CreateUserCommand)
export class CreateUserHandler implements ICommandHandler<CreateUserCommand, Result> {
    constructor(
        @InjectRepository(User)
        private readonly repository: Repository<User>
    ){}
    
    public async execute(command: CreateUserCommand): Promise<Result<unknown>> {
        const { payload } = command;

        if (payload.email === "not-accept@email.com") {
            return Result.Failure(Reason.NotAcceptable());
        }

        const emailHasBeenTaken = await this.repository.count({
            where: {
                email: Equal(payload.email)
            }
        });
        if (emailHasBeenTaken) {
            return Result.Failure(Reason.Conflict());
        }

        if (payload.email === "exception@email.com") {
            throw new Error("Unexpected error occurred while creating user.");
        }

        return Result.Success();
    }
}