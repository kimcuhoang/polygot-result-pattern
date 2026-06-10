import { IQueryResult } from "@nestjs/cqrs";
import { ApiProperty } from "@nestjs/swagger";

export class FindUserResult implements IQueryResult {
    @ApiProperty({ description: "The unique identifier of the user." })
    id: string;
    @ApiProperty({ description: "The email address of the user." })
    email: string;
    
    constructor(options: Partial<FindUserResult>) {
        Object.assign(this, options);
    }
}