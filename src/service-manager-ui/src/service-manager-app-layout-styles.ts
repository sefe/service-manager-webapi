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

import "@vaadin/vaadin-lumo-styles/color.js";
import "@vaadin/vaadin-lumo-styles/spacing.js";
import "@vaadin/vaadin-lumo-styles/typography.js";
import {
  css,
  registerStyles,
} from "@vaadin/vaadin-themable-mixin/vaadin-themable-mixin.js";

registerStyles(
  "vaadin-app-layout",
  css`
    [part="navbar"]::before {
      background: var(--lumo-base-color)
        linear-gradient(var(--lumo-contrast-5pct), var(--lumo-contrast-5pct));
    }

    [part="drawer"],
    :host([overlay]) [part="drawer"]::before {
      width: 120px;
      height: var(--_vaadin-app-layout-drawer-scroll-size, 100%);
      background: var(--lumo-base-color);
    }

    :host(:not([dir="rtl"]):not([overlay])) [part="drawer"] {
      border-right: 1px solid var(--lumo-contrast-10pct);
    }

    :host([dir="rtl"]:not([overlay])) [part="drawer"] {
      border-left: 1px solid var(--lumo-contrast-10pct);
    }

    :host([overlay]) [part="drawer"]::before {
      background: var(--lumo-base-color);
    }

    [part="navbar"]::before,
    :host([overlay]) [part="drawer"]::before {
      position: absolute;
      z-index: -1;
      width: 100%;
      height: 100%;
      content: "";
    }

    [part="backdrop"] {
      background-color: var(--lumo-shade-20pct);
      opacity: 1;
    }

    [part] ::slotted(h2),
    [part] ::slotted(h3),
    [part] ::slotted(h4) {
      margin-top: var(--lumo-space-xs) !important;
      margin-bottom: var(--lumo-space-xs) !important;
    }

    @supports (-webkit-backdrop-filter: blur(1px)) or
      (backdrop-filter: blur(1px)) {
      [part="navbar"]::before {
        opacity: 0.8;
      }

      [part="navbar"] {
        -webkit-backdrop-filter: blur(24px);
        backdrop-filter: blur(24px);
      }

      :host([overlay]) [part="drawer"]::before {
        opacity: 0.9;
      }

      :host([overlay]) [part="drawer"] {
        -webkit-backdrop-filter: blur(24px);
        backdrop-filter: blur(24px);
      }
    }
  `,
  { moduleId: "lumo-app-layout" },
);
