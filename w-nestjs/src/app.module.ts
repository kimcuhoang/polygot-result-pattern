import { Module } from '@nestjs/common';
import { AppController } from './app.controller';
import { AppService } from './app.service';
import { CqrsModule } from '@nestjs/cqrs';
import { CommandHandlers } from './use-cases/commands';
import { APP_FILTER, APP_INTERCEPTOR } from '@nestjs/core';
import { ResponseInterceptor } from './infrastructures/interceptors/response.interceptor';
import { GlobalExceptionFilter } from './infrastructures/filters/global-exception.filter';
import { QueryHandlers } from './use-cases/queries';
import { TypeOrmModule } from '@nestjs/typeorm';
import { ConfigModule, ConfigService } from '@nestjs/config';
import { getPostgresConnectionOptions } from './infrastructures/typeorm/datasource.properties';
import { DataSource, DataSourceOptions } from 'typeorm';
import { addTransactionalDataSource, getDataSourceByName } from 'typeorm-transactional';
import { UserSchema } from './infrastructures/typeorm/schemas/user.schema';

@Module({
	imports: [
		ConfigModule.forRoot({
			isGlobal: true,
		}),
		CqrsModule.forRoot(),
		TypeOrmModule.forFeature([
			UserSchema
		]),
		TypeOrmModule.forRootAsync({
			inject: [ ConfigService ],
			useFactory: (configService: ConfigService) => ({
				...getPostgresConnectionOptions(configService),
			}),
			dataSourceFactory: async (options: DataSourceOptions) => {
				const dataSource = getDataSourceByName("default") ?? new DataSource(options);
				return await Promise.resolve(addTransactionalDataSource(dataSource));
			}
		})
	],
	controllers: [AppController],
	providers: [
		AppService,
		...Object.values(CommandHandlers),
		...Object.values(QueryHandlers),
		{
			provide: APP_INTERCEPTOR,
			useClass: ResponseInterceptor
		},
		{
			provide: APP_FILTER,
			useClass: GlobalExceptionFilter
		}
	],
})
export class AppModule { }
