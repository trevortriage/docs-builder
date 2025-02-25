// Licensed to Elasticsearch B.V under one or more agreements.
// Elasticsearch B.V licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information

module ``container elements``.``vertical definition lists``

open Xunit
open authoring

type ``simple multiline definition with markup`` () =

    static let markdown = Setup.Markdown """
This is my `definition`
:   And this is the definition **body**

    Which may contain multiple lines
    """

    [<Fact>]
    let ``validate HTML`` () =
        markdown |> convertsToHtml """
             <dl>
                <dt>This is my <code>definition</code> </dt>
                <dd>
                    <p> And this is the definition <strong>body</strong></p>
                    <p>Which may contain multiple lines</p>
                </dd>
             </dl>
            """
    [<Fact>]
    let ``has no errors`` () = markdown |> hasNoErrors

type ``with embedded directives`` () =

    static let markdown = Setup.Markdown """
This is my `definition`
:   And this is the definition **body**
    Which may contain multiple lines
    :::{note}
    My note
    :::
"""

    [<Fact>]
    let ``validate HTML 2`` () =
        markdown |> convertsToHtml """
              <dl>
 	            <dt>This is my
 		            <code>definition</code>
 	            </dt>
 	            <dd>
             		<p>And this is the definition
 			            <strong>body</strong>
 			            <br>
 			            Which may contain multiple lines
 			        </p>
 		            <div class="admonition note">
            			<div class="admonition-title">
            				<svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" class="size-6">
            					<path stroke-linecap="round" stroke-linejoin="round" d="m11.25 11.25.041-.02a.75.75 0 0 1 1.063.852l-.708 2.836a.75.75 0 0 0 1.063.853l.041-.021M21 12a9 9 0 1 1-18 0 9 9 0 0 1 18 0Zm-9-3.75h.008v.008H12V8.25Z"></path>
            				</svg>
             				<span>Note</span>
             			</div>
             			<div class="admonition-content">
 				            <p>My note</p>
             			</div>
 		            </div>
 	            </dd>
             </dl>
            """
    [<Fact>]
    let ``has no errors 2`` () = markdown |> hasNoErrors



type ``preserves paragraphs`` () =

    static let markdown = Setup.Markdown """
Elastic Consumption Unit (ECU)
:   An ECU is a unit of aggregate consumption across multiple resources over time.

    Each type of computing resource (capacity, data transfer, and snapshot) that you consume has its own unit of measure.

    In order to aggregate consumption across different resource types, all resources are priced in ECU.

    Check Using Elastic Consumption Units for billing for more details.
"""

    [<Fact>]
    let ``validate HTML 2`` () =
        markdown |> convertsToHtml """
        <dl>
            <dt>Elastic Consumption Unit (ECU)</dt>
            <dd>
                <p>An ECU is a unit of aggregate consumption across multiple resources over time.</p>
                <p>Each type of computing resource (capacity, data transfer, and snapshot) that you consume has its own unit of measure.</p>
                <p>In order to aggregate consumption across different resource types, all resources are priced in ECU.</p>
                <p>Check Using Elastic Consumption Units for billing for more details.</p>
            </dd>
        </dl>
        """
    [<Fact>]
    let ``has no errors`` () = markdown |> hasNoErrors
