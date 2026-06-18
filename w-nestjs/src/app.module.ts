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
import { LoggerModule, PinoLogger } from 'nestjs-pino';
import { TypeOrmPinoLogger } from 'typeorm-pino-logger';
import { LevelWithSilentOrString } from 'pino';
import pino from 'pino'; // 🚀 Import core pino for multistream
import { ApplicationInsightStream } from './infrastructures/loggers/app-insights-stream';

@Module({
	imports: [
		ConfigModule.forRoot({
			isGlobal: true,
		}),
		CqrsModule.forRoot(),
		TypeOrmModule.forFeature([
			UserSchema
		]),
		LoggerModule.forRootAsync({
			providers: [ApplicationInsightStream],
			inject: [ConfigService, ApplicationInsightStream],
			useFactory: (configService: ConfigService, applicationInsightStream: ApplicationInsightStream) => {
				const logLevel = (configService.get<string>('PINO_LOG_LEVEL')?.toLocaleLowerCase() ?? 'error') as LevelWithSilentOrString;
				const applicationInsightStreamInstance = applicationInsightStream.configApplicationInsightsStream();

				const pinoHttp = {
					enabled: true,
					level: logLevel,
					redact: {
						remove: true,
						paths: [
							'req.id',
							'req.remoteAddress',
							'req.remotePort',
							'req.headers.authorization',
							'req.headers.cookie',
							'req.headers[x-forwarded-for]',
							'req.headers[host]',
							'req.headers[connection]',
							'res.headers.set-cookie',
							'res.headers[x-powered-by]'
						],
					}
				};

				return {
					stream: applicationInsightStreamInstance ?? process.stdout,
					pinoHttp: {
						...pinoHttp,
						transport: configService.get<string>('PINO_LOG_PRETTY')?.toLocaleLowerCase() === 'true'
							? { target: 'pino-pretty', options: { colorize: true, levelFirst: true } }
							: undefined,
					}
				};
			}
		}),
		TypeOrmModule.forRootAsync({
			inject: [ConfigService, PinoLogger],
			useFactory: (configService: ConfigService, pinoLogger: PinoLogger) => {

				const pinoLogLevel = configService.get<string>('PINO_LOG_LEVEL')?.toLocaleLowerCase() ?? 'error';
				const options = getPostgresConnectionOptions(configService);
				let pinoTypeOrmLogLevel = pinoLogLevel;

				if (!["error", "warn"].includes(pinoLogLevel) && (options.logging === true || options.logging === "all")) {
					pinoTypeOrmLogLevel = "debug";
				}

				return {
					...getPostgresConnectionOptions(configService),
					logger: new TypeOrmPinoLogger(
						pinoLogger.logger.child({}, { level: pinoTypeOrmLogLevel }),
						{
							logQueries: true,
							logMigrations: true,
							logQueryErrors: true,
							logSlowQueries: true,
							logSchemaOperations: true,
							slowQueryThreshold: 1000, // Log queries that take longer than 1000ms
						})
				};
			},
			dataSourceFactory: async (options: DataSourceOptions) => {
				const dataSource = getDataSourceByName("default")
					?? addTransactionalDataSource(new DataSource(options));
				return await Promise.resolve(dataSource);
			}
		}),

	],
	controllers: [AppController],
	providers: [
		AppService,
		...Object.values(CommandHandlers),
		...Object.values(QueryHandlers),
		// ApplicationInsightStream,
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
