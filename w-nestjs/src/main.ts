import { NestFactory } from '@nestjs/core';
import { AppModule } from './app.module';
import { BadRequestException, ValidationPipe } from '@nestjs/common';
import { DocumentBuilder, SwaggerModule } from '@nestjs/swagger';
import { initializeTransactionalContext, StorageDriver } from 'typeorm-transactional';
import { Logger } from 'nestjs-pino';
import { ConfigService } from '@nestjs/config';
import * as appInsights from 'applicationinsights';

const azureAppInsight = () => {
	const configService = new ConfigService();
	const connectionString = configService.get<string>('AZURE_APPLICATION_INSIGHT_CONNECTION_STRING');
	if (!connectionString) {
		return;
	}

	appInsights.setup(connectionString)
		.setDistributedTracingMode(appInsights.DistributedTracingModes.AI_AND_W3C)
		.setAutoDependencyCorrelation(true)
		.setAutoCollectRequests(true) // Tracks incoming requests
		.setAutoCollectPerformance(true, true)
		.setAutoCollectExceptions(true)
		.setAutoCollectDependencies(false) // Tracks outgoing DB / HTTP calls
		.start();
};

async function bootstrap() {

	initializeTransactionalContext({
		storageDriver: StorageDriver.ASYNC_LOCAL_STORAGE
	});

	const app = await NestFactory.create(AppModule, { 
		bufferLogs: true
	});

	const config = new DocumentBuilder()
		.setTitle('Users API')
		// .setDescription('The cats API description')
		.setVersion('1.0')
		// .addTag('cats')
		.build();
	const documentFactory = () => SwaggerModule.createDocument(app, config);
	SwaggerModule.setup('swagger', app, documentFactory);


	app.useGlobalPipes(new ValidationPipe({
		whitelist: true,
		forbidNonWhitelisted: true,
		transform: true,
		exceptionFactory: (errors) => {
			const messages = errors.map(error => {
				const constraints = Object.values(error.constraints ?? {});
				return {
					[error.property]: constraints
				}
			});

			throw new BadRequestException(messages);
		},
	}));

	app.useLogger(app.get(Logger));
	await app.listen(process.env.PORT ?? 3000);
}


azureAppInsight();

bootstrap();
