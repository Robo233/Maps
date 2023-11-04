import xml.etree.ElementTree as ET

# Load the original OSM file
tree = ET.parse('C:/Users/Robi/Desktop/map.osm')
root = tree.getroot()

# Create a new XML tree for the roads
roads_tree = ET.ElementTree(ET.Element('osm'))
roads_root = roads_tree.getroot()

# Copy over the bounds element
bounds = root.find('bounds')
if bounds is not None:
    roads_root.append(bounds)

# Identify all nodes referenced by the road elements
node_ids = set()
for way in root.findall('way'):
    for tag in way.findall('tag'):
        if tag.attrib.get('k') == 'highway':
            for nd in way.findall('nd'):
                node_ids.add(nd.attrib.get('ref'))
            break

# Copy over the node elements that are referenced by the road elements
for node in root.findall('node'):
    if node.attrib.get('id') in node_ids:
        roads_root.append(node)

# Copy over the road elements
for way in root.findall('way'):
    for tag in way.findall('tag'):
        if tag.attrib.get('k') == 'highway':
            roads_root.append(way)
            break

# Save the new XML tree to a file
roads_tree.write('C:/Users/Robi/Desktop/new_map.osm', encoding='utf-8', xml_declaration=True)

