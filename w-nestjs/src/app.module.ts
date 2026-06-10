import { Module } from '@nestjs/common';
import { AppController } from './app.controller';
import { AppService } from './app.service';
import { CqrsModule } from '@nestjs/cqrs';
import { CommandHandlers } from './use-cases/commands';
import { APP_FILTER, APP_INTERCEPTOR } from '@nestjs/core';
import { ResponseInterceptor } from './interceptors/response.interceptor';
import { GlobalExceptionFilter } from './filters/global-exception.filter';
import { QueryHandlers } from './use-cases/queries';

@Module({
  imports: [
    CqrsModule.forRoot()
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
export class AppModule {}
