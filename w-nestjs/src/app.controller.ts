import { Body, Controller, Get, HttpStatus, Param, Post, Query, Redirect } from '@nestjs/common';
import { AppService } from './app.service';
import { CommandBus, QueryBus } from '@nestjs/cqrs';
import { ApiBody, ApiConflictResponse, ApiNotAcceptableResponse, ApiParam, ApiQuery, ApiResponse } from '@nestjs/swagger';
import { CreateUserCommand, CreateUserPayload } from './use-cases/commands';
import { FindUserCriteria, FindUserQuery, FindUserResult } from './use-cases/queries';

@Controller()
export class AppController {
  constructor(
    private readonly appService: AppService,
    private readonly commandBus: CommandBus,
    private readonly queryBus: QueryBus
  ) { }

  @Get()
  @Redirect("/swagger", HttpStatus.PERMANENT_REDIRECT)
  gotoSwagger() { }

  @Post("/users")
  @ApiBody({ type: CreateUserPayload })
  @ApiResponse({ status: HttpStatus.CREATED, description: 'The user has been successfully created.' })
  @ApiConflictResponse({ description: 'The user already exists.'})
  @ApiNotAcceptableResponse({ description: 'The payload is not valid.' })
  public async createUser(@Body() payload: CreateUserPayload): Promise<void> {
    return await this.commandBus.execute(new CreateUserCommand(payload));
  }

  @Get("/users")
  // @ApiQuery({ name: "criteria", type: FindUserCriteria, description: "The criteria to find the user." })
  @ApiResponse({ status: HttpStatus.OK, description: 'The user details have been successfully retrieved.', type: FindUserResult })
  @ApiResponse({ status: HttpStatus.NOT_FOUND, description: 'The user with the specified identifier was not found.' })
  public async getUserDetails(@Query() criteria: FindUserCriteria): Promise<FindUserResult> {
    const result = await this.queryBus.execute(new FindUserQuery(criteria));
    return result;
  }

}
