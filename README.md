# JSON2UGUI
 Convert JSON data to create UGUI templates.

<h2> Tool Workflow </h2>
<ol>
 <li>This tool has an option to create a new Template Container i.e. JSON file containing the list of templates.</li>
 <li>The newly created template will directly get fetched into JSON path, so user can Load it.</li>
 <li>Only after Loading a valid JSON file, it's root template objects will be displayed.</li>
 <li>User can add, edit, delete, instantiate(spawn) the root template objects and their individual child object templates individually.</li>
</ol>

<h2> How To Use </h2>
<ol>
 <li>Open the template editor by going to Window -> UI Object Template Editor</li>
 <li>User has two options after launching the editor
  <ol>
   <li>Create a new UI Object Template Container(JSON) file to store Object Template data.</li>
   <li>Load an existing Container(JSON) to add, edit, delete, and/or save the existing content.</li>
  </ol>
 </li>
 <li>Load a JSON by providing a valid path or creating a new one, by pressing the 'Load JSON' button.</li>
 <li>On retracting the foldout next to the templates, it will display the list of root templates.</li>
 <li>We can add a new UI Object Trmplate to the root templates list by pressing the New Object Template button.</li>
 <li>We can similarly remove an UI object template from the root list as well, by pressing the '-' button.</li>
 <li>We can replicate the whole UI Object Template creation and removal process for the nested objects, which will add and/or remove them to the children list of the corresponding object respectively.</li>
 <li>We can instantiate any root object template or nested object template, by pressing the Spawn bhtton.</li>
</ol>

<h2> Assumptions </h2>
<ol>
 <li>Each UI Object Template corrsponds to a JSON object which can only be stored inside the UIObjectTemplatesData array. Similarly it also corresponds to a UGUI gameobject.</li>
 <li>Each UI Object Template replicates the same properties that of RectTransform i.e. name, templateType, position, size, minAnchor, maxAnchor, pivot, rotateion, and wxale, so they can be applied when instantiated in the scene.</li>
 <li>When a spawn button is pressed for any UI Object Template, it will first check if a canvas is present in the scene or not, if yes, then all the instantiated template will get attached to it, otherwise a new Canvas is created and then the above process is done.</li>
 <li>The JSON file contians an array named UIObjectsTemplates, where all the root UIObjectTemplate objects are stored as elements of the array.</li>
</ol>
