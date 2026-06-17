export class User {
    id: string;
    name: string;
    email: string;

    constructor(data: Partial<User>) {
        Object.assign(this, data);
    }

    public static defaultUser = new User({
        id: "01KVB4Y5WYQGFYCSKSRST0GT85",
        email: "jdoe@email.com",
        name: "John Doe"
    })
}