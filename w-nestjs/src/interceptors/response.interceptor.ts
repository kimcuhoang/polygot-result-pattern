import {
	Injectable,
	NestInterceptor,
	ExecutionContext,
	CallHandler,
	HttpException,
	HttpStatus,
} from '@nestjs/common';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { Reflector } from '@nestjs/core';
import { ReasonType, Result } from 'src/models';
import { Request, Response } from 'express';

@Injectable()
export class ResponseInterceptor<T> implements NestInterceptor {
	constructor(
		private reflector: Reflector
	) { }

	intercept(context: ExecutionContext, next: CallHandler): Observable<any> {
		return next.handle().pipe(
			map((result: unknown) =>
				this.responseHandler(result, context)),
				// catchError((err: HttpException) =>
				//   throwError(() => this.errorHandler(err, context)),
				// ),
		);
	}

	responseHandler(res: any, context: ExecutionContext): unknown {
		const ctx = context.switchToHttp();
		const response = ctx.getResponse();
		const request = ctx.getRequest();

		if (!(res instanceof Result)) {
          return res;
        }

		const result = res as Result;
		if (result.isSuccess) {
			return res.data;
		}

		const { reason } = result;
		const statusCode: number = this.getStatusCode(reason.reasonType);

		response
			.status(statusCode)
			.contentType('application/problem+json')
			.json({
				title: this.getStatusTitle(statusCode),
				type: `https://tools.ietf.org/html/rfc9110#section-15.${this.getRfcSection(statusCode)}`,
				status: statusCode,
				path: request.url,
				detail: reason.message,
				traceId: request.headers['x-request-id'] ?? `00-${this.generateTraceId()}-00`,
			});
	}

	errorHandler(exception: HttpException, context: ExecutionContext) {
		const ctx = context.switchToHttp();
		const response = ctx.getResponse<Response>();
		const request = ctx.getRequest<Request>();

		const exceptionResponse = exception.getResponse();
		response
			.status(exception.getStatus() ?? HttpStatus.INTERNAL_SERVER_ERROR)
			.json({
				path: request.url,
				...(typeof exceptionResponse === 'object'
					? exceptionResponse
					: { message: exceptionResponse })
			});
	}

	private getStatusCode(reasonType: ReasonType): number {
		switch (reasonType) {
			case "Conflict":
				return HttpStatus.CONFLICT;
			case "NotFound":
				return HttpStatus.NOT_FOUND;
			case "Forbidden":
				return HttpStatus.FORBIDDEN;
			case "NotAcceptable":
				return HttpStatus.NOT_ACCEPTABLE
			default:
				return HttpStatus.EXPECTATION_FAILED;
		}
	}

	private getStatusTitle(status: number): string {
		switch (status) {
			case HttpStatus.BAD_REQUEST: return 'Bad Request';
			case HttpStatus.NOT_ACCEPTABLE: return 'Not acceptable';
			case HttpStatus.NOT_FOUND: return 'Not Found';
			case HttpStatus.CONFLICT: return 'Conflict';
			case HttpStatus.FORBIDDEN: return 'Forbidden';
			default: return 'Expectation Failed';
		}
	}

	private getRfcSection(status: number): string {
		switch (status) {
			case HttpStatus.BAD_REQUEST: return '5.1';
			case HttpStatus.FORBIDDEN: return '5.4';
			case HttpStatus.NOT_FOUND: return '5.5';
			case HttpStatus.CONFLICT: return '5.10';
			case HttpStatus.NOT_ACCEPTABLE: return '5.7';
			default: return '5.18';
		}
	}

	private generateTraceId(): string {
		return Math.random().toString(36).substring(2, 15) + Math.random().toString(36).substring(2, 15);
	}
}