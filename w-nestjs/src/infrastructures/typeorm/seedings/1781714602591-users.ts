import { User } from '@src/domains/user';
import { DataSource, Equal } from 'typeorm';
import { Seeder, SeederFactoryManager } from 'typeorm-extension';
import { UserSchema } from '../schemas/user.schema';

export class Users1781714602591 implements Seeder {
    track = true;

    public async run(
        dataSource: DataSource,
        factoryManager: SeederFactoryManager
    ): Promise<any> {
        const repository = dataSource.getRepository(UserSchema);

        const existingUser = await repository.findOne({
            where: {
                id: Equal(User.defaultUser.id)
            }
        });

        if (!existingUser) {
            await repository.save(User.defaultUser);
        }
    }
}
