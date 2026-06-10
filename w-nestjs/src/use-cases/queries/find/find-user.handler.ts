import { IQueryHandler, QueryHandler } from "@nestjs/cqrs";
import { FindUserQuery } from "./find-user.query";
import { FindUserResult } from "./find-user.result";
import { Reason, Result } from "src/models";


@QueryHandler(FindUserQuery)
export class FindUserHandler implements IQueryHandler<FindUserQuery, Result<FindUserResult>> {

    public async execute(query: FindUserQuery): Promise<Result<FindUserResult>> {

        const { criteria } = query;

        if (criteria.email === "notfound@email.com") {
            return new Promise((resolve) => {
                const result = Result.Failure<FindUserResult>(Reason.NotFound());
                resolve(result);
            });
        }

        return new Promise((resolve) => {
            const result = Result.Success<FindUserResult>(new FindUserResult({
                id: "OK",
                email: "success@email.com"
            }));

            resolve(result);
        });
    }
}