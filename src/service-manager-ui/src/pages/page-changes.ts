/**
 * Copyright 2024 SEFE Securing Energy for Europe GmbH
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

import { css, LitElement, PropertyValues, render } from "lit";
import { customElement, property, state } from "lit/decorators.js";
import { html } from "lit/html.js";
import {
  ApprovalStatus,
  ChangeRequest,
  ChangesApi,
} from "../apis/service-manager-api";
import "@vaadin/grid";
import "@vaadin/grid/vaadin-grid-sort-column.js";
import {
  GridActiveItemChangedEvent,
  GridColumn,
  GridItemModel,
} from "@vaadin/grid";
import "@vaadin/horizontal-layout";
import "@vaadin/vertical-layout";
import "../components/change-controls";
import { Notification } from "@vaadin/notification";
import { gridRowDetailsRenderer } from "@vaadin/grid/lit.js";
import { unsafeHTML } from "lit/directives/unsafe-html.js";
import { WarningNotification } from "../components/warning-notification.ts";
import linkifyHtml from "linkify-html";

let me = "";

@customElement("page-changes")
export class PageChanges extends LitElement {
  @property({ type: Boolean })
  private loading = true;

  @property({ type: Array }) changeRequests: Array<ChangeRequest> = [];

  @state()
  private detailsOpenedItem: ChangeRequest[] = [];
  @property({ type: Boolean }) noResults = false;

  static get styles() {
    return css`
      vaadin-grid#grid {
        overflow: hidden;
        height: calc(100vh - 55px);
        --divider-color: rgb(223, 232, 239);
      }

      .vaadin-button-container {
        justify-content: center;
      }

      vaadin-grid-cell-content {
        padding-top: 0px;
        padding-bottom: 0px;
        margin: 0px;
      }

      .overlay {
        width: 100%;
        height: 100%;
        position: fixed;
      }

      .overlay__inner {
        width: 100%;
        height: 100%;
        position: absolute;
      }

      .overlay__content {
        left: 20%;
        position: absolute;
        top: 20%;
        transform: translate(-50%, -50%);
      }

      .spinner {
        width: 75px;
        height: 75px;
        display: inline-block;
        border-width: 2px;
        border-color: rgba(255, 255, 255, 0.05);
        border-top-color: cornflowerblue;
        animation: spin 1s infinite linear;
        border-radius: 100%;
        border-style: solid;
      }

      @keyframes spin {
        100% {
          transform: rotate(360deg);
        }
      }

      .main-box {
        display: flex;
      }

      .box-of-text-title {
        margin: 10px 0 3px 20px;
        color: var(--lumo-primary-color-50pct);
        display: inline-block;
      }

      .box-of-text {
        display: flex;
        flex-direction: column;
        flex-wrap: wrap;
        margin: 0 20px;
        font-family: monospace;
        font-size: medium;
        white-space: pre-line;
        border-radius: 15px;
        border: 2px solid cornflowerblue;
        padding: 10px;
      }

      .box-of-text * {
        overflow-wrap: anywhere;
      }

      .box-of-text a {
        margin: 0 5px;
      }

      .box-of-text__table {
        display: grid;
        line-height: 0.8rem;
      }

      .table__line,
      .table__line-title {
        display: flex;
        flex-wrap: wrap;
        margin-top: 5px;
      }

      .table__line-title {
        margin: 0;
      }

      .table__line-title * {
        color: var(--lumo-primary-color-50pct);
        margin-right: 10px;
      }

      .table__line > div,
      .table__line-title > div {
        width: 200px;
        line-height: 0.8rem;
      }

      @media only screen and (max-width: 1150px) {
        .main-box {
          flex-direction: column;
        }

        .table__line > div,
        .table__line-title > div {
          width: 45%;
        }
      }
    `;
  }

  constructor() {
    super();

    const api = new ChangesApi();
    api.changesGetMe().subscribe(
      (data: string) => {
        me = data;
      },

      (err: any) => console.error(err),
      () => console.log("done loading display name"),
    );

    api.changesGetMyApprovals().subscribe(
      (data: ChangeRequest[]) => {
        this.setChangeRequests(data);
      },

      (err: any) => console.error(err),
      () => console.log("done loading change requests"),
    );
  }

  setChangeRequests(changeRequests: ChangeRequest[]) {
    this.changeRequests = changeRequests;
    if (this.changeRequests.length === 0) {
      this.noResults = true;
    }
    this.loading = false;
  }

  render() {
    return html`
      <div class="overlay" style="z-index: 2" ?hidden="${!this.loading}">
        <div class="overlay__inner">
          <div class="overlay__content">
            <span class="spinner"></span>
          </div>
        </div>
      </div>
      <vaadin-grid
        id="grid"
        .items=${this.changeRequests}
        column-reordering-allowed
        multi-sort
        .detailsOpenedItems="${this.detailsOpenedItem}"
        .cellClassNameGenerator="${this.cellClassNameGenerator}"
        style="z-index: 1"
        theme="compact row-stripes no-row-borders no-border"
        ?hidden="${this.loading || this.noResults}"
        @active-item-changed="${(
          event: GridActiveItemChangedEvent<ChangeRequest>,
        ) => {
          const change = event.detail.value;
          this.detailsOpenedItem = change ? [change] : [];
        }}"
        ${gridRowDetailsRenderer<ChangeRequest>(
          (change) => html`
            <div class="main-box">
              <div style="margin: 10px 0 0 0; flex-grow: 1;">
                <span class="box-of-text-title">Description</span>
                <div class="box-of-text">
                  ${unsafeHTML(this.linkify(change.Description ?? ""))}
                </div>
              </div>
              <div style="margin: 10px 0 0 0;">
                <span class="box-of-text-title">Approvers</span>
                <div class="box-of-text" style="flex-direction: column">
                  <div class="box-of-text__table">
                    <div class="table__line-title">
                      <div>Approver Name</div>
                      <div>Approver Status</div>
                    </div>
                    ${change.ApprovalStatuses?.sort(this.compareApprovers)?.map(
                      (v) =>
                        html` <div class="table__line">
                          <div style="margin-right: 10px;">${v.Approver}</div>
                          <div>${v.Status}</div>
                        </div>`,
                    )}
                  </div>
                </div>
              </div>
            </div>
            <div style="margin: 0 0 20px 0;">
              <span class="box-of-text-title">Reason</span>
              <div class="box-of-text">
                ${unsafeHTML(this.linkify(change.Reason ?? ""))}
              </div>
              ${!this.isEmpty(change.Notes ?? "")
                ? html`
                    <span class="box-of-text-title">Notes</span>
                    <div class="box-of-text">
                      ${unsafeHTML(this.linkify(change.Notes ?? ""))}
                    </div>
                  `
                : html``}
              ${!this.isEmpty(change.ImplementationPlan ?? "")
                ? html`
                    <span class="box-of-text-title">Implementation Plan</span>
                    <div class="box-of-text">
                      ${unsafeHTML(
                        this.linkify(change.ImplementationPlan ?? ""),
                      )}
                    </div>
                  `
                : html``}
              ${!this.isEmpty(change.BackoutPlan ?? "")
                ? html`
                    <span class="box-of-text-title">Back Out Plan</span>
                    <div class="box-of-text">
                      ${unsafeHTML(this.linkify(change.BackoutPlan ?? ""))}
                    </div>
                  `
                : html``}
            </div>
          `,
          [],
        )}
      >
        <vaadin-grid-column
          .renderer="${this._changeRequestsButtonsRenderer}"
          width="200px"
          flex-grow="0"
          direction="asc"
          resizable
        ></vaadin-grid-column>
        <vaadin-grid-sort-column
          path="Id"
          header="ID"
          resizable
          width="120px"
          flex-grow="0"
          direction="asc"
        ></vaadin-grid-sort-column>
        <vaadin-grid-sort-column
          path="Title"
          header="Title"
          resizable
        ></vaadin-grid-sort-column>
        <vaadin-grid-sort-column
          path="Priority"
          header="Priority"
          resizable
          auto-width
          flex-grow="0"
        ></vaadin-grid-sort-column>
        <vaadin-grid-sort-column
          path="Impact"
          header="Impact"
          resizable
          auto-width
          flex-grow="0"
        ></vaadin-grid-sort-column>
        <vaadin-grid-column
          resizable
          .renderer="${this.timingsRenderer}"
          header="Timings"
          auto-width
          flex-grow="0"
        ></vaadin-grid-column>
        <vaadin-grid-sort-column
          path="CreatedBy"
          header="Created By"
          resizable
          auto-width
          flex-grow="0"
        ></vaadin-grid-sort-column>
      </vaadin-grid>
      <img
        class="cover"
        style="z-index: 2"
        ?hidden="${!this.noResults}"
        src="${"/1200-to-do-nothing.jpg"}"
        alt="No Results Found"
      />
    `;
  }

  linkify(inputText: string) {
    return linkifyHtml(inputText, { });
  }

  compareApprovers(a: ApprovalStatus, b: ApprovalStatus) {
    const c = a.Approver ?? "";
    const d = b.Approver ?? "";

    if (c < d) {
      return -1;
    }

    if (c > d) {
      return 1;
    }

    return 0;
  }

  isEmpty(str: string) {
    return !str || str.length === 0;
  }

  protected firstUpdated(_changedProperties: PropertyValues) {
    super.firstUpdated(_changedProperties);

    this.addEventListener(
      "approve-change-request",
      this.approveChangeRequest as EventListener,
    );
    this.addEventListener(
      "reject-change-request",
      this.rejectChangeRequest as EventListener,
    );
    this.addEventListener(
      "clear-change-request",
      this.clearChangeRequest as EventListener,
    );
  }

  _changeRequestsButtonsRenderer(
    root: HTMLElement,
    _column: HTMLElement,
    { item }: GridItemModel<ChangeRequest>,
  ) {
    const changeRequest = item as ChangeRequest;
    render(
      html` <change-controls
        .changeRequest="${changeRequest}"
      ></change-controls>`,
      root,
    );
  }

  private cellClassNameGenerator(
    _: GridColumn,
    model: GridItemModel<ChangeRequest>,
  ) {
    const item = model.item;
    let parts = "";

    const status = item.ApprovalStatuses?.filter(
      (status) => me === status.Approver,
    )[0];

    if (status !== undefined) {
      if (status.Status === "Approved") {
        parts += " approved";
      }

      if (status.Status === "Rejected") {
        parts += " rejected";
      }
    }
    console.log("found cr with " + parts);
    return parts;
  }

  private timingsRenderer = (
    root: HTMLElement,
    _: HTMLElement,
    model: GridItemModel<ChangeRequest>,
  ) => {
    const request = model.item as ChangeRequest;
    let sTime = "",
      sDate = "";
    let cTime = "",
      cDate = "";

    const start = new Date(request.ScheduledStartDate ?? "");
    const end = new Date(request.ScheduledEndDate ?? "");

    if (start !== undefined) {
      sTime = start?.toLocaleTimeString("en-GB");
      sDate = start?.toLocaleDateString("en-GB", {
        day: "2-digit",
        month: "2-digit",
        year: "numeric",
      });
    }
    if (end !== undefined) {
      cTime = end?.toLocaleTimeString("en-GB");
      cDate = end?.toLocaleDateString("en-GB", {
        day: "2-digit",
        month: "2-digit",
        year: "numeric",
      });
    }

    render(
      html`
        <vaadin-horizontal-layout style="align-items: center;" theme="spacing">
          <vaadin-vertical-layout
            style="line-height: var(--lumo-line-height-s);"
          >
            <div>${sDate + " " + sTime}</div>
            <div>${cDate + " " + cTime}</div>
          </vaadin-vertical-layout>
        </vaadin-horizontal-layout>
      `,
      root,
    );
  };

  private rejectChangeRequest(e: CustomEvent) {
    const changeRequest = e.detail.Change as ChangeRequest;

    const api = new ChangesApi();
    api.changesPutRejectChange({ id: changeRequest.Id ?? "" }).subscribe({
      next: (value) => {
        this.notifyWarnUser(value, changeRequest);
      },
      error: (err) => {
        console.error(err);
      },
      complete: () => {},
    });
  }

  private refreshChangeList(changeRequest: ChangeRequest) {
    const api = new ChangesApi();
    api.changesGet({ id: changeRequest.Id ?? "" }).subscribe({
      next: (cr) => {
        const index = this.changeRequests.findIndex(
          (value1) => value1.Id === changeRequest.Id,
        );
        if (index > -1) {
          // only splice array when item is found
          this.changeRequests.splice(index, 1); // 2nd parameter means remove one item only
        }
        this.changeRequests.push(cr);

        this.changeRequests = JSON.parse(JSON.stringify(this.changeRequests));
      },
      error: (err) => {
        console.error(err);
      },
      complete: () => {},
    });
  }

  private approveChangeRequest(e: CustomEvent) {
    const changeRequest = e.detail.Change as ChangeRequest;

    const api = new ChangesApi();
    api.changesPutApproveChange({ id: changeRequest.Id ?? "" }).subscribe({
      next: (value) => {
        this.notifyUser(value, changeRequest, "Approved change request: ");
      },
      error: (err) => {
        console.error(err);
      },
      complete: () => {},
    });
  }

  private clearChangeRequest(e: CustomEvent) {
    const changeRequest = e.detail.Change as ChangeRequest;

    const api = new ChangesApi();
    api.changesPutClearApproveStatus({ id: changeRequest.Id ?? "" }).subscribe({
      next: (value) => {
        this.notifyUser(value, changeRequest, "Removed Status from change request: ");
      },
      error: (err) => {
        console.error(err);
      },
      complete: () => {},
    });
  }

  private notifyUser(value: boolean, changeRequest: ChangeRequest, notifyMsg: string) {
    if (value) {
      Notification.show(notifyMsg + changeRequest.Id, {
        theme: "success",
        position: "bottom-end",
        duration: 5000,
      });

      this.refreshChangeList(changeRequest);
    } else {
      console.log("Failed to " + notifyMsg + changeRequest.Id);
    }
  }

  private notifyWarnUser(value: boolean, changeRequest: ChangeRequest) {
    if (value) {
      const notification = new WarningNotification();
      notification.setAttribute(
          "warningMessage",
          "Rejected change request: " + changeRequest.Id,
      );
      this.shadowRoot?.appendChild(notification);
      notification.open();

      this.refreshChangeList(changeRequest);
    } else {
      console.log("Failed to reject change request: " + changeRequest.Id);
    }
  }
}
