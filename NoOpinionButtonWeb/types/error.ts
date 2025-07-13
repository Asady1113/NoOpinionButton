export enum ApiErrorType {
  BadRequest = 'BadRequest',
  Unauthorized = 'Unauthorized',
  NotFound = 'NotFound',
  Server = 'Server',
}

export interface ApiError {
  type: ApiErrorType
  message: string
  statusCode: number
}