import React from "react";

export function RatingItem(data) {
    
    const item = data.item

    const rowSelect = () => {
        window.location.replace('https://vk.com/id'+item.Id); 
    }
    return(        
            <tr onClick = {rowSelect}>
                <td>{item.Id}</td>
                <td>{item.Name}</td>
                <td>{item.Lvl}</td>
                <td>{item.Strength} ({item.itemStrength})</td>
            </tr>        
    )
    
}
