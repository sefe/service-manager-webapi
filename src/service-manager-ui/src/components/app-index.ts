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

import { css, html, LitElement } from "lit";
import { customElement } from "lit/decorators.js";
import "@vaadin/app-layout";
import "@vaadin/app-layout/vaadin-drawer-toggle";
import "@vaadin/icon";
import "@vaadin/icons";
import "@vaadin/tabs";
import "../service-manager-app-layout-styles.ts";

@customElement("app-index")
export class AppIndex extends LitElement {
  render() {
    return html`
      <vaadin-app-layout>
        <vaadin-drawer-toggle slot="navbar"></vaadin-drawer-toggle>
        <h2 slot="navbar">Service Manager</h2>
        <vaadin-tabs slot="drawer" orientation="vertical">
          <vaadin-tab>
            <a href="changes">
              <vaadin-icon icon="vaadin:cart"></vaadin-icon>
              Approvals
            </a>
          </vaadin-tab>
        </vaadin-tabs>
        <slot></slot>
      </vaadin-app-layout>
    `;
  }

  static styles = css`
    :host {
      display: flex;
      flex-direction: column;
      font-family: var(--lumo-font-family);
    }

    main,
    main > * {
      display: flex;
      flex: 1;
      flex-direction: column;
    }

    main:empty ~ footer {
      display: none;
    }

    vaadin-icon {
      padding-right: 0.2em;
      width: var(--lumo-icon-size-s);
      height: var(--lumo-icon-size-s);
    }

    a {
      color: inherit; /* blue colors for links too */
      text-decoration: inherit; /* no underline */
      padding-top: 2px;
      padding-bottom: 2px;
    }

    a.plain {
      text-decoration: underline;
      color: blue;
    }

    vaadin-tab {
      padding-top: 0px;
      padding-bottom: 0px;
    }
  `;
}
