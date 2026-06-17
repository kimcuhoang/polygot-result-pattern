import { IQueryHandler, QueryHandler } from "@nestjs/cqrs";
import { FindUserQuery } from "./find-user.query";
import { FindUserResult } from "./find-user.result";
import { Reason, Result } from "src/models";
import { InjectRepository } from "@nestjs/typeorm";
import { User } from "@src/domains/user";
import { Equal, Repository } from "typeorm";


@QueryHandler(FindUserQuery)
export class FindUserHandler implements IQueryHandler<FindUserQuery, Result<FindUserResult>> {

    constructor(
        @InjectRepository(User)
        private readonly repository: Repository<User>
    ){}

    public async execute(query: FindUserQuery): Promise<Result<FindUserResult>> {

        const { criteria } = query;

        const user = await this.repository.findOne({
            where: {
                email: Equal(criteria.email)
            }
        });

        if (!user) {
            return Result.Failure<FindUserResult>(Reason.NotFound());
        }

        return Result.Success<FindUserResult>(new FindUserResult(user));
    }
}