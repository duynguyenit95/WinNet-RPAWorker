var script = document.createElement('script');
script.src = 'https://code.jquery.com/jquery-3.6.0.min.js';
document.getElementsByTagName('head')[0].appendChild(script);

// Wait Jquery loaded
// Expand SAP GUI Scripting API => Objects 
// Expand SAP GUI Scripting API => Enumerations 

function ReadMethods() {
    let methods = [];
    let rows = $('.topic-wrapper').find('section[type="Methods"]').children('div').children('div.table-wrapper').children('table').children('tbody').children('tr');
    $.each(rows, (i, row) => {
        let jRow = $(row);
        let cols = jRow.find('td');
        // Inherit Methods
        if (cols.length == 1) {
            let inhertObj = cols.find('a.xref').text();
            let inheritMethod = cols.find('li');
            $.each(inheritMethod, (ih, ihItem) => {
                let obj = {
                    InheritFrom: inhertObj.replace('Object', '').trim(),
                    MethodName: $(ihItem).text()
                }
                methods.push(obj);
            })
        }
        // Self Methods
        else {
            let obj = {};
            $.each(cols, (c, col) => {
                let jCol = $(col);
                if (c == 0) {
                    obj.MethodName = jCol.find('kbd').text();
                    let ctor = jCol.find('.pre.codeblock').text();
                    if (ctor) {
                        
                        obj.Contructor = ctor;
                        let paramctor = ctor.substring(ctor.indexOf("(") + 1, ctor.indexOf(")") - 1)
                        obj.Params = [];
                        if (paramctor !== "(") {
                            $.each(paramctor.split(','), (i, pa) => {
                                let param = {};
                                let paSplit = pa.split(' ').filter(x => x !== "_" && x !== "" && x !== "_\n");
                                param.Type = paSplit[paSplit.length - 1];
                                param.Name = paSplit[paSplit.length - 3];
                                param.ArgumentPassingType = paSplit[paSplit.length - 4];
                                param.IsOptional = paSplit.includes("Optional");
                                obj.Params.push(param)
                            })
                        }
                        let returnPart = ctor.substr(ctor.indexOf(")") + 1);
                        let spReturnPart = returnPart.split(' ');
                        obj.ReturnType = spReturnPart[spReturnPart.length - 1];
                    }
                }
                else {
                    obj.Description = jCol.html();
                }
            });
            methods.push(obj);
        }

    });
    return methods;
};

function ReadProperties() {
    let properties = [];
    let rows = $('.topic-wrapper').find('section[type="Properties"]').children('div').children('div.table-wrapper').children('table').children('tbody').children('tr');
    $.each(rows, (i, row) => {
        let jRow = $(row);
        let cols = jRow.find('td');
        // Inherit Methods
        if (cols.length == 1) {
            let inhertObj = cols.find('a.xref').text();
            let inheritMethod = cols.find('li');
            $.each(inheritMethod, (ih, ihItem) => {
                let obj = {
                    InheritFrom: inhertObj.replace('Object', '').trim(),
                    PropertyName: $(ihItem)[0].innerText
                }
                properties.push(obj);
            })
        }
        // Self Methods
        else {
            let obj = {};
            $.each(cols, (c, col) => {
                let jCol = $(col);
                if (c == 0) {
                    obj.PropertyName = jCol.find('kbd').text();
                    obj.Access = jCol.clone()    //clone the element
                        .children() //select all the children
                        .remove()   //remove all the children
                        .end()  //again go back to selected element
                        .text().split('\n').join("").split('\t').join("").split(' ').join("");
                    obj.Contructor = jCol.find('.pre.codeblock').text();
                    let splitCtor = obj.Contructor.split(' ');
                    obj.PropertyType = splitCtor[splitCtor.length - 1];
                }
                else {
                    obj.Description = jCol.html();
                }
            });
            properties.push(obj);
        }

    });
    return properties;
}

function ReadMembers() {
    let members = [];
    let rows = $('.topic-wrapper').find('table').children('tbody').children('tr');
    $.each(rows, (i, row) => {
        let jRow = $(row);
        let obj = {};
        let cols = jRow.find('td');
        $.each(cols, (c, col) => {
            let jCol = $(col);
            
            // Member
            if (c == 0) {
                obj.Member = jCol.text();
            }
            // Value
            else if (c == 1) {
                obj.Value = jCol.text();
            }
            // Description might not exists
            else if (c == 2) {
                obj.Description = jCol.text();
            }
        });
        members.push(obj);
    });
    return members;
}




function ChangeTopic(list, index) {
    if (list.length - 1 > index) {
        let jItem = $(list[index]);
        console.log(jItem);
        if (jItem.text().trim().startsWith('Gui')) {
            jItem[0].click();
            setTimeout(() => {
                let title = $('#topic-title').text();
                let obj = {
                    Document : $('.topic-wrapper').html()
                };

                if (title.includes("Object")) {
                    obj.Name = title.replace('Object', '').trim();
                    obj.Type = "Object";
                }
                else if (title.includes("Collection")) {
                    obj.Name = title.replace(' Collection', '').trim();
                    obj.Type = "Collection";
                }
                else {
                    obj.Name = title.trim();
                }

                if ($('.topic-wrapper').find('section').length > 0) {
                    obj.Properties = ReadProperties();
                    obj.Methods = ReadMethods();
                }
                else {
                    obj.Members = ReadMembers();
                    obj.Type = "Enum"
                }

                res.push(obj);
                ChangeTopic(list, index + 1);
            }, 2000)
        }
        else {
            ChangeTopic(list, index + 1);
        }

    }
    else {
        console.log("Done");
        console.log(res);
    }
}
let res = [];
function ParseObject() {

    let list = $('a[data-v-cf04166e]');
    ChangeTopic(list, 0);
    JSON.stringify(res);
}

ParseObject()

