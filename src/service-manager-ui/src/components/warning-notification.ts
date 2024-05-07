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

import "@vaadin/notification";
import { css, LitElement, render } from "lit";
import { customElement, property, state } from "lit/decorators.js";
import { html } from "lit/html.js";
import {
  NotificationOpenedChangedEvent,
  NotificationRenderer,
} from "@vaadin/notification";

@customElement("warning-notification")
export class WarningNotification extends LitElement {
  @state()
  private notificationOpened = false;
  @property({ type: String })
  private warningMessage = "";

  static get styles() {
    return css``;
  }

  render() {
    return html`
      <vaadin-notification
        id="error-toast"
        theme="warning"
        duration="5000"
        position="bottom-end"
        .opened="${this.notificationOpened}"
        @opened-changed="${(e: NotificationOpenedChangedEvent) => {
          this.notificationOpened = e.detail.value;
        }}"
        .renderer="${this.errorNotificationRenderer}"
      ></vaadin-notification>
    `;
  }

  errorNotificationRenderer: NotificationRenderer = (root) => {
    render(
      html`
        <vaadin-horizontal-layout theme="spacing" style="align-items: start;">
          <div>${this.warningMessage}</div>
          <vaadin-button
            theme="tertiary-inline"
            @click="${() => (this.notificationOpened = false)}"
            aria-label="Close"
          >
            <vaadin-icon icon="lumo:cross"></vaadin-icon>
          </vaadin-button>
        </vaadin-horizontal-layout>
      `,
      root,
    );
  };

  public open() {
    this.notificationOpened = true;
  }
}
