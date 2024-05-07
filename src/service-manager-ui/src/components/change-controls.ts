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

import { css, LitElement } from "lit";
import "@vaadin/button";
import "@vaadin/icon";
import "@vaadin/icons";
import "@vaadin/vaadin-lumo-styles/icons.js";
import { customElement, property } from "lit/decorators.js";
import { html } from "lit/html.js";
import { ChangeRequest } from "../apis/service-manager-api";
import AppConfig from "../app-config.ts";

@customElement("change-controls")
export class ChangeControls extends LitElement {
  @property({ type: Object }) changeRequest: ChangeRequest | undefined;

  static get styles() {
    return css`
      vaadin-button {
        padding: 5px;
        margin: 2px;
        display: inline-block;
      }
      vaadin-icon {
        width: var(--lumo-icon-size-m);
        height: var(--lumo-icon-size-m);
      }
    `;
  }

  render() {
    return html`
      <vaadin-button
        title="Approve Change"
        theme="icon"
        @click="${this.approveChangeRequest}"
        label="approve"
      >
        <vaadin-icon
          icon="lumo:checkmark"
          style="color: cornflowerblue"
        ></vaadin-icon>
      </vaadin-button>
      <vaadin-button
        title="Clear Approval"
        theme="icon"
        @click="${this.clearChangeRequest}"
        label="approve"
      >
        <vaadin-icon
          icon="vaadin:eraser"
          style="color: cornflowerblue"
        ></vaadin-icon>
      </vaadin-button>
      <vaadin-button
        title="Reject Change"
        theme="icon"
        @click="${this.rejectChangeRequest}"
        label="approve"
      >
        <vaadin-icon
          icon="vaadin:ban"
          style="color: cornflowerblue"
        ></vaadin-icon>
      </vaadin-button>
      <vaadin-button
        title="Open Change"
        theme="icon"
        @click="${this.openChangeDetails}"
      >
        <vaadin-icon
          icon="vaadin:ellipsis-dots-h"
          style="color: cornflowerblue"
        ></vaadin-icon>
      </vaadin-button>
    `;
  }
  approveChangeRequest(_: CustomEvent) {
    this.setApprovalStatusEventRequest("approve");
  }
  clearChangeRequest(_: CustomEvent) {
    this.setApprovalStatusEventRequest("clear");
  }
  rejectChangeRequest(_: CustomEvent) {
    this.setApprovalStatusEventRequest("reject");
  }

  setApprovalStatusEventRequest(eventType: string) {
    const event = new CustomEvent(eventType + "-change-request", {
      detail: {
        Change: this.changeRequest,
      },
      bubbles: true,
      composed: true,
    });
    this.dispatchEvent(event);
  }

  openChangeDetails() {
    window.open(new AppConfig().serviceDeskUrlChangePrefix + this.changeRequest?.Id,
    );
  }
}
