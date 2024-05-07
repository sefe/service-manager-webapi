// tslint:disable
/**
 * Trading.ServiceManagerUtils.Api
 * No description provided (generated by Openapi Generator https://github.com/openapitools/openapi-generator)
 *
 * The version of the OpenAPI document: v1
 *
 *
 * NOTE: This class is auto generated by OpenAPI Generator (https://openapi-generator.tech).
 * https://openapi-generator.tech
 * Do not edit the class manually.
 */

import type { ApprovalStatus, ImpactQuestions, RiskQuestions } from "./";

/**
 * @export
 * @interface ChangeRequest
 */
export interface ChangeRequest {
  /**
   * @type {string}
   * @memberof ChangeRequest
   */
  Id?: string;
  /**
   * @type {string}
   * @memberof ChangeRequest
   */
  Title?: string;
  /**
   * @type {string}
   * @memberof ChangeRequest
   */
  Description?: string;
  /**
   * @type {string}
   * @memberof ChangeRequest
   */
  Reason?: string;
  /**
   * @type {string}
   * @memberof ChangeRequest
   */
  Area?: string;
  /**
   * @type {string}
   * @memberof ChangeRequest
   */
  Priority?: string;
  /**
   * @type {string}
   * @memberof ChangeRequest
   */
  Impact?: string;
  /**
   * @type {string}
   * @memberof ChangeRequest
   */
  Risk?: string;
  /**
   * @type {string}
   * @memberof ChangeRequest
   */
  RiskAssessmentPlan?: string;
  /**
   * @type {string}
   * @memberof ChangeRequest
   */
  Notes?: string;
  /**
   * @type {string}
   * @memberof ChangeRequest
   */
  SupportGroup?: string;
  /**
   * @type {string}
   * @memberof ChangeRequest
   */
  ScheduledStartDate?: string;
  /**
   * @type {string}
   * @memberof ChangeRequest
   */
  ScheduledEndDate?: string;
  /**
   * @type {string}
   * @memberof ChangeRequest
   */
  ImplementationPlan?: string;
  /**
   * @type {string}
   * @memberof ChangeRequest
   */
  TestPlan?: string;
  /**
   * @type {string}
   * @memberof ChangeRequest
   */
  BackoutPlan?: string;
  /**
   * @type {string}
   * @memberof ChangeRequest
   */
  Status?: string;
  /**
   * @type {string}
   * @memberof ChangeRequest
   */
  CRStatus?: string;
  /**
   * @type {string}
   * @memberof ChangeRequest
   */
  ChangeLevel?: string;
  /**
   * @type {string}
   * @memberof ChangeRequest
   */
  Customer?: string;
  /**
   * @type {Array<string>}
   * @memberof ChangeRequest
   */
  Approvers?: Array<string>;
  /**
   * @type {ImpactQuestions}
   * @memberof ChangeRequest
   */
  ImpactQuestionResponses?: ImpactQuestions;
  /**
   * @type {RiskQuestions}
   * @memberof ChangeRequest
   */
  RiskQuestionResponses?: RiskQuestions;
  /**
   * @type {string}
   * @memberof ChangeRequest
   */
  TemplateName?: string;
  /**
   * @type {string}
   * @memberof ChangeRequest
   */
  InitialActivityToComplete?: string;
  /**
   * @type {string}
   * @memberof ChangeRequest
   */
  CreatedBy?: string;
  /**
   * @type {Array<ApprovalStatus>}
   * @memberof ChangeRequest
   */
  ApprovalStatuses?: Array<ApprovalStatus>;
}
