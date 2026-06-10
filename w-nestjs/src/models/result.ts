import { Reason } from "./reason";

export class Result<T = unknown> {
    isSuccess: boolean;
    data?: T;
    reason?: Reason;

    constructor(options: Partial<Result<T>>) {
        Object.assign(this, options);
    }

    public static Success<T>(data?: T): Result<T> {
        return new Result<T>({
            isSuccess: true,
            data,
        });
    }

    public static Failure<T>(reason: Reason): Result<T> {
        return new Result<T>({
            isSuccess: false,
            reason: reason,
        });
    }
}