
export type ReasonType = 'NotFound' | 'Conflict' | 'NotAcceptable' | 'Forbidden';

export class Reason {
    reasonType: ReasonType;
    message: string;

    constructor(options: Partial<Reason>) {
        Object.assign(this, options);
    }

    public static NotFound(message: string = "Some things are not found"): Reason {
        return new Reason({
            reasonType: 'NotFound',
            message,
        });
    }

    public static Conflict(message: string = "Some things are in conflict with the current state of the system"): Reason {
        return new Reason({
            reasonType: 'Conflict',
            message,
        });
    }

    public static NotAcceptable(message: string = "Some things are not valid"): Reason {
        return new Reason({
            reasonType: 'NotAcceptable',
            message,
        });
    }

    public static Forbidden(message: string = "Access denied. Insufficient permissions."): Reason {
        return new Reason({
            reasonType: 'Forbidden',
            message,
        });
    }
}